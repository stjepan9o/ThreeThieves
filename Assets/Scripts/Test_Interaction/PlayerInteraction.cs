using UnityEngine;
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public ActionPointManager apManager;

    [Header("Settings")]
    public float maxInteractionDistance = 20f;
    public LayerMask interactableLayer;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (apManager == null)
        {
            apManager = FindObjectOfType<ActionPointManager>();
            if (apManager == null)
                Debug.LogError("ActionPointManager nije pronadjen u sceni! Dodaj GameManager objekt.");
        }
    }

    void Update()
    {
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();


            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<InteractableObject>();

            if (interactable != null)
            {
                Debug.Log($"Kliknuto na: {hit.collider.gameObject.name} ({interactable.interactionPrompt})");
                interactable.Interact();
            }
        }
    }


    void OnDrawGizmos()
    {
        if (cam == null) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray.origin, ray.direction * maxInteractionDistance);
    }
}
