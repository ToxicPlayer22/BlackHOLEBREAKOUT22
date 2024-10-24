using UnityEngine;

public class BlackHolePull : MonoBehaviour
{
    public Transform blackHole; // Reference to the black hole's transform
    public float pullStrength = 5f; // Strength of the pull
    public float minSize = 0.01f; // Minimum size of the object
    public float pullRadius = 5f; // Radius for pull effect
    public float shrinkStartDistance = 2f; // Distance at which shrinking begins
    public float growthPercentage = 0.01f; // Growth percentage for black hole
    public float shrinkSpeed = 0.1f; // Speed of shrinking
    public float consumeDistanceThreshold = 0.1f; // Distance to consume the object

    void Update()
    {
        // Find all objects within the pull radius
        Collider[] objectsInRange = Physics.OverlapSphere(blackHole.position, pullRadius);
        
        foreach (Collider col in objectsInRange)
        {
            if (col.CompareTag("Attractable")) // Ensure it's an attractable object
            {
                // Calculate direction to the black hole
                Vector3 direction = (blackHole.position - col.transform.position).normalized;

                // Move the object towards the black hole
                col.transform.position += direction * pullStrength * Time.deltaTime;

                // Check distance to the black hole
                float distance = Vector3.Distance(blackHole.position, col.transform.position);

                // Start shrinking if within distance
                if (distance < shrinkStartDistance)
                {
                    // Shrink the object
                    float newSize = col.transform.localScale.x - (shrinkSpeed * Time.deltaTime);
                    col.transform.localScale = new Vector3(Mathf.Max(newSize, minSize), Mathf.Max(newSize, minSize), Mathf.Max(newSize, minSize));

                    // Check if the object should be consumed
                    if (newSize <= minSize || distance < consumeDistanceThreshold)
                    {
                        ConsumeObject(col);
                    }
                }
            }
        }
    }

    private void ConsumeObject(Collider col)
    {
        // Update the black hole's scale
        float growthAmount = transform.localScale.x * growthPercentage;
        transform.localScale += new Vector3(growthAmount, growthAmount, growthAmount);

        // Destroy the object
        Destroy(col.gameObject);
    }
}
