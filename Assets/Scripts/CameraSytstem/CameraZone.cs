using UnityEngine;

public class CameraZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Player usao u zonu kamere");
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Player izasao iz zone kamere");
    }
}