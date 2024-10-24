using UnityEngine;
using System.Collections;

public class BlackHole : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 100f;   // Detection radius for orbiting
    [SerializeField] private float suckInRadius = 3f;        // Sucking radius
    [SerializeField] private float baseSuckInSpeed = 0.5f; 
    [SerializeField] private float instantSuckInRadius = 2f; 
    [SerializeField] private float minSizeFactor = 0.05f;    
    [SerializeField] private float maxSizeFactor = 1f; // Max size factor (100%)
    [SerializeField] private float gravitationalForce = 10f; // Adjusted gravitational force
    [SerializeField] private float orbitSpeed = 2f; // Speed of orbiting
    [SerializeField] private float maxOrbitSpeed = 2f; // Max speed limit for orbiting
    [SerializeField] private float maxOrbitDistance = 10f; // Max distance from the black hole

    private void FixedUpdate()
    {
        DetectAndProcessAttractableObjects();
    }

    private void DetectAndProcessAttractableObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Attractable"))
            {
                if (collider.TryGetComponent<Rigidbody>(out Rigidbody rb) && !rb.isKinematic)
                {
                    StartCoroutine(HandleAttractable(collider.transform, rb));
                }
            }
        }
    }

    private IEnumerator HandleAttractable(Transform objectToSuck, Rigidbody rb)
    {
        float orbitTime = 0f; // Timer for orbiting
        bool hasStartedSucking = false; // Flag for sucking state

        while (objectToSuck != null)
        {
            float distance = Vector3.Distance(transform.position, objectToSuck.position);
            Vector3 directionToBlackHole = (transform.position - objectToSuck.position).normalized;

            // Calculate the gravitational force
            Vector3 gravity = directionToBlackHole * gravitationalForce / (distance * distance + 1f); // Avoid division by zero

            // Calculate orbiting position
            Vector3 orbitPosition = Quaternion.Euler(0, orbitSpeed * Time.fixedDeltaTime, 0) * (objectToSuck.position - transform.position);
            objectToSuck.position = transform.position + orbitPosition;

            // Apply gravitational pull
            rb.AddForce(gravity);

            // Ensure the object stays within the max orbit distance
            if (distance > maxOrbitDistance)
            {
                objectToSuck.position = transform.position + directionToBlackHole * maxOrbitDistance;
            }

            // Clamp the speed of the Rigidbody
            Vector3 velocity = rb.velocity;
            if (velocity.magnitude > maxOrbitSpeed)
            {
                rb.velocity = velocity.normalized * maxOrbitSpeed;
            }

            // Adjust size based on distance
            float scaleReductionFactor = Mathf.Clamp01(1 - (distance / suckInRadius));
            objectToSuck.localScale = Vector3.one * Mathf.Lerp(maxSizeFactor, minSizeFactor, scaleReductionFactor);

            // Increment orbit timer
            orbitTime += Time.fixedDeltaTime;

            // Check if within suck-in radius
            if (distance < suckInRadius && orbitTime >= 2f && !hasStartedSucking)
            {
                hasStartedSucking = true;
                SuckInObject(objectToSuck, distance);
                
                // Check if the object is at the center of the black hole
                if (distance < 0.1f)
                {
                    Destroy(objectToSuck.gameObject); // Object disappears at the center
                    yield break; // Exit the coroutine
                }
            }
            else if (distance < suckInRadius)
            {
                EnableRagdoll(objectToSuck);
                break; // Exit if out of sucking range
            }

            yield return null; // Wait for the next frame
        }
    }

    private void EnableRagdoll(Transform objectToSuck)
    {
        if (objectToSuck == null) return; // Additional check

        if (objectToSuck.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void SuckInObject(Transform objectToSuck, float distance)
    {
        if (objectToSuck == null) return; // Additional check

        // Increased pulling speed based on distance
        float pullSpeed = baseSuckInSpeed * Mathf.Clamp01(1 - (distance / suckInRadius)); 
        objectToSuck.position = Vector3.MoveTowards(objectToSuck.position, transform.position, pullSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suckInRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxOrbitDistance); // Draw the max orbit distance
    }
}
