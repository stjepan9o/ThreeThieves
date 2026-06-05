using UnityEngine;

//TO DO: Test with OnTriggerStay later when we define logic

public class CameraZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player usao u zonu kamere");

            InfiltratorAbility abilities = other.GetComponent<InfiltratorAbility>();
            if (abilities != null)
            {
                if (abilities.isHidden)
                {
                    return;
                }
                Debug.Log("Kamera vidi igrača!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Player izasao iz zone kamere");
    }
}