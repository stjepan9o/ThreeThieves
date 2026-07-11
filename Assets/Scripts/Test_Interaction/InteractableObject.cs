using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionPrompt = "Interact";
    public int apCost = 1;

    [Header("Interaction Range")]
    [Tooltip("Maksimalna udaljenost (u poljima/jedinicama) s koje aktivni lik moze interaktirati. " +
             "Ako je predaleko, lik ce sam dohodati do objekta prije interakcije.")]
    public float interactionRange = 2.5f;
    public void Interact()
    {
        OnInteract();
    }
    
    protected virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name} je interaktiran.");
    }
}
