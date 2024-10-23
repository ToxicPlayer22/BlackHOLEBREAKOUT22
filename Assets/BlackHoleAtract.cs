using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public float gravityStrength = 10f; // Adjust this value for stronger/weaker pull
    public float attractionRadius = 5f; // Radius within which objects are attracted

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Attractable"))
        {
            // Calculate direction to the black hole
            Vector3 direction = (transform.position - other.transform.position).normalized;

            // Calculate distance to the black hole
            float distance = Vector3.Distance(transform.position, other.transform.position);

            // Only apply gravity if within attraction radius
            if (distance < attractionRadius)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Apply a force towards the black hole
                    rb.AddForce(direction * gravityStrength / distance, ForceMode.Acceleration);
                }
            }
        }
    }
}