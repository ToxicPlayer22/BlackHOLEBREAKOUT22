using UnityEngine;
using System.Collections;

public class BlackHole : MonoBehaviour
{
    public float detectionRadius = 50f;   // Detection radius
    public float floatStrength = 0.5f;     // Strength for upward movement
    public float suckInRadius = 5f;        // Suck-in radius
    public float suckInSpeed = 0.1f;       // Speed of sucking in objects
    public float minSizeFactor = 0.05f;    // Minimum size factor (5% of original size)

    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Attractable"))
            {
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);

                    // Start levitation animation before pulling
                    if (distance < suckInRadius + 1f)
                    {
                        StartCoroutine(LevitateAndSuckIn(collider.transform));
                    }
                }
            }
        }
    }

    private IEnumerator LevitateAndSuckIn(Transform objectToSuck)
    {
        // Levitation phase
        float levitationDuration = 3f;
        float elapsedTime = 0f;

        Vector3 originalPosition = objectToSuck.position;

        while (elapsedTime < levitationDuration)
        {
            // Controlled upward movement
            objectToSuck.position = Vector3.Lerp(originalPosition, originalPosition + Vector3.up * 0.5f, elapsedTime / levitationDuration);
            
            // Tilt towards the black hole
            Vector3 directionToBlackHole = (transform.position - objectToSuck.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToBlackHole);
            objectToSuck.rotation = Quaternion.Slerp(objectToSuck.rotation, targetRotation, Time.deltaTime * 2f); // Smoothly tilt towards the black hole

            // Gentle movement towards the black hole
            objectToSuck.position += directionToBlackHole * 0.01f; // Gentle push towards the black hole

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Enable ragdoll physics if applicable
        if (objectToSuck.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false; // Disable kinematic to allow physics
            rb.useGravity = true;   // Enable gravity for ragdoll effect
        }

        // Begin sucking the object in
        Vector3 originalScale = objectToSuck.localScale;
        while (objectToSuck != null)
        {
            float distance = Vector3.Distance(transform.position, objectToSuck.position);
            float scaleReductionFactor = Mathf.Clamp01(1 - (distance / (suckInRadius + 1))); // Normalize distance

            // Calculate new scale
            objectToSuck.localScale = originalScale * Mathf.Lerp(1f, minSizeFactor, scaleReductionFactor);

            // Move the object towards the black hole at a slow speed
            objectToSuck.position = Vector3.MoveTowards(objectToSuck.position, transform.position, suckInSpeed * Time.deltaTime);

            // Tilt towards the black hole while being sucked in
            Vector3 directionToBlackHole = (transform.position - objectToSuck.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToBlackHole);
            objectToSuck.rotation = Quaternion.Slerp(objectToSuck.rotation, targetRotation, Time.deltaTime * 2f); // Smoothly tilt towards the black hole

            // Break if the object is within suck-in radius
            if (distance < suckInRadius)
                break;

            yield return null;
        }

        // Check again before destroying the object
        if (objectToSuck != null)
        {
            Destroy(objectToSuck.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suckInRadius);
    }
}
