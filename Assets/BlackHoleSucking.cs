using UnityEngine;
using System.Collections;

public class BlackHole : MonoBehaviour
{
    public float detectionRadius = 50f;       // Detection radius for attracting objects
    public float suckInRadius = 1f;            // Sucking radius (smaller to allow closer approach)
    public float gravitationalForce = 0.002f;   // Very weak gravitational force for slow attraction
    public float orbitDistance = 7f;            // Distance from the black hole for orbiting
    public float minOrbitDistance = 2f;         // Minimum distance while orbiting
    public float orbitSpeed = 0.5f;              // Faster speed of orbiting
    public float timeToSuckIn = 5f;              // Time to fully get sucked into the black hole
    public float orbitDuration = 5f;              // Duration for orbiting
    public float delayBeforeSuckIn = 3f;        // Delay before allowing sucking to start
    public float suckInSpeed = 0.01f;            // Speed of sucking the object into the black hole
    public float orbitShrinkRate = 0.01f;        // Rate at which the orbit distance shrinks

    private bool abilityActive = false;          // To track if the ability is active
    private Transform currentObject;              // To track the currently grabbed object

    private void Update()
    {
        // Check for left mouse button input
        if (Input.GetMouseButtonDown(0))
        {
            abilityActive = true; // Activate the ability
        }
        else if (Input.GetMouseButtonUp(0))
        {
            abilityActive = false; // Deactivate the ability
            if (currentObject != null)
            {
                Rigidbody rb = currentObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false; // Allow ragdoll effect or regular physics
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (abilityActive)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Attractable"))
                {
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        StartCoroutine(HandleAttractable(collider.transform, rb));
                    }
                }
            }
        }
    }

    private IEnumerator HandleAttractable(Transform objectToSuck, Rigidbody rb)
    {
        float orbitTimer = 0f; 
        bool canSuckIn = false; 
        bool isOrbiting = false;

        while (objectToSuck != null)
        {
            currentObject = objectToSuck; // Track the currently grabbed object
            float distance = Vector3.Distance(transform.position, objectToSuck.position);

            if (distance < suckInRadius && canSuckIn)
            {
                SuckInObject(objectToSuck);
                yield break; 
            }

            if (distance < minOrbitDistance)
            {
                Vector3 inwardDirection = (transform.position - objectToSuck.position).normalized;
                float pullStrength = gravitationalForce * Time.fixedDeltaTime; 
                objectToSuck.position += inwardDirection * pullStrength; 
            }
            else if (distance < orbitDistance && distance > minOrbitDistance)
            {
                if (!isOrbiting)
                {
                    orbitTimer = 0f; 
                    isOrbiting = true;
                }

                if (orbitTimer >= delayBeforeSuckIn)
                {
                    canSuckIn = true;
                }

                SmoothOrbitAroundBlackHole(objectToSuck);
            }
            else
            {
                Vector3 directionToBlackHole = (transform.position - objectToSuck.position).normalized;
                rb.AddForce(directionToBlackHole * gravitationalForce * rb.mass);
            }

            if (isOrbiting)
            {
                orbitTimer += Time.fixedDeltaTime;

                if (orbitTimer >= orbitDuration)
                {
                    isOrbiting = false; 
                }

                // Slowly shrink orbit distance
                if (orbitDistance > minOrbitDistance)
                {
                    orbitDistance -= orbitShrinkRate * Time.deltaTime;
                }
            }

            yield return null; 
        }
    }

    private void SmoothOrbitAroundBlackHole(Transform objectToSuck)
    {
        float angle = Time.fixedTime * orbitSpeed; 
        Vector3 targetPosition = transform.position + new Vector3(Mathf.Cos(angle) * orbitDistance, 0, Mathf.Sin(angle) * orbitDistance);
        
        // Smoothly interpolate the position
        objectToSuck.position = Vector3.Lerp(objectToSuck.position, targetPosition, Time.fixedDeltaTime * 2f); // Adjust the smoothing factor as needed
    }

    private void SuckInObject(Transform objectToSuck)
    {
        objectToSuck.position = Vector3.MoveTowards(objectToSuck.position, transform.position, suckInSpeed * Time.deltaTime);
        Destroy(objectToSuck.gameObject, 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suckInRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, orbitDistance); 

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minOrbitDistance); 
    }
}
