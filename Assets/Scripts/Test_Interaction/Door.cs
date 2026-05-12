using UnityEngine;

// Dodaj ovu skriptu na Cube koji predstavlja vrata
// Nasljeduje InteractableObject - automatski dobiva klik detekciju i AP provjeru
public class Door : InteractableObject
{
    [Header("Door Settings")]
    public bool disappearOnOpen = true;   // true = nestanu, false = animacija otvaranja
    public float openAngle = 90f;         // Kut rotacije ako koristis animaciju

    private bool isOpen = false;

    void Start()
    {
        interactionPrompt = "Razbij vrata";
        apCost = 2; // Muscle Man trosi 2 AP za razbijanje vrata
    }

    protected override void OnInteract()
    {
        if (isOpen)
        {
            Debug.Log("Vrata su vec otvorena!");
            return;
        }

        isOpen = true;
        Debug.Log($"{gameObject.name}: Vrata su razbijena!");

        if (disappearOnOpen)
        {
            // Vrata nestaju
            gameObject.SetActive(false);
        }
        else
        {
            // Vrata se otvaraju rotacijom
            transform.Rotate(0f, openAngle, 0f);
        }
    }

    // Vizualni indikator u editoru - vrata su zelena kad su zatvorena
    void OnDrawGizmos()
    {
        Gizmos.color = isOpen ? Color.gray : Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
