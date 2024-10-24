using System.Collections.Generic;
using UnityEngine;

public class AbilityNumber1 : MonoBehaviour
{
    public float maxPullForce = 20f; // Maximum strength of the pull
    public float pullRange = 15f; // Range within which objects can be attracted
    public float minPullForce = 5f; // Minimum strength of the pull at max range
    private List<Rigidbody> attractableObjects = new List<Rigidbody>();
    private bool isPulling = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            isPulling = true;
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isPulling = false;
        }

        if (isPulling)
        {
            AttractObjects();
        }
    }

    void AttractObjects()
    {
        foreach (Rigidbody obj in attractableObjects)
        {
            // Calculate the distance from the black hole
            float distance = Vector3.Distance(transform.position, obj.position);

            // Check if the object is within pull range
            if (distance < pullRange)
            {
                // Calculate the direction towards the black hole
                Vector3 direction = (transform.position - obj.position).normalized;

                // Calculate pull strength based on distance (closer = stronger)
                // Using a power function to allow for gradual pulling
                float strength = Mathf.Lerp(minPullForce, maxPullForce, Mathf.Pow(1 - (distance / pullRange), 2));

                // Apply a force towards the black hole
                obj.AddForce(direction * strength);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attractable")) // Check if the object has the "Attractable" tag
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !attractableObjects.Contains(rb))
            {
                attractableObjects.Add(rb); // Add the rigidbody to the list
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Attractable")) // Check if the object has the "Attractable" tag
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                attractableObjects.Remove(rb); // Remove the rigidbody from the list
            }
        }
    }
}
