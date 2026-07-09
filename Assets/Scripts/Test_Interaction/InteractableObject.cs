using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionPrompt = "Interact";  
    public int apCost = 1;                         
    public void Interact()
    {
        OnInteract();
    }
    
    protected virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name} je interaktiran.");
    }
}
