using UnityEngine;

// Bazna klasa za sve interaktivne objekte u igri
// Dodaj ovu skriptu (ili njene nasljednike) na bilo koji objekt koji zelimo kliknuti
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionPrompt = "Interact";  // Tekst koji se prikazuje igracu
    public int apCost = 1;                         // Koliko AP kosta interakcija

    // Ovu metodu poziva PlayerInteraction kad igrac klikne na objekt
    public void Interact()
    {
        OnInteract();
    }
    // Override ovu metodu u nasljednicima (npr. Door.cs)
    protected virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name} je interaktiran.");
    }
}
