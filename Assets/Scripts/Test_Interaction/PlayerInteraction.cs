using UnityEngine;

// Postavi ovu skriptu na Main Camera
// Detektira klik misem na bilo koji InteractableObject u sceni
// Ne koristi hardkodirane reference - radi genericko na kliknuti objekt!
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public ActionPointManager apManager; // Povuci GameManager objekt ovdje u Inspectoru

    [Header("Settings")]
    public float maxInteractionDistance = 20f; // Maksimalna udaljenost interakcije
    public LayerMask interactableLayer;         // Postavi na "Default" ili napravi novi layer

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Auto-pronadji ActionPointManager ako nije assignan
        if (apManager == null)
        {
            apManager = FindObjectOfType<ActionPointManager>();
            if (apManager == null)
                Debug.LogError("ActionPointManager nije pronadjen u sceni! Dodaj GameManager objekt.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Lijevi klik
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance))
        {
            // Provjeri ima li kliknuti objekt InteractableObject komponentu
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                Debug.Log($"Kliknuto na: {hit.collider.gameObject.name} ({interactable.interactionPrompt})");
                interactable.Interact(apManager);
            }
        }
    }

    // Debug - prikazuje raycast u Scene viewu
    void OnDrawGizmos()
    {
        if (cam == null) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray.origin, ray.direction * maxInteractionDistance);
    }
}
