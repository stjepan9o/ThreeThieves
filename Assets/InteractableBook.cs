using UnityEngine;

public class InteractableBook : InteractableObject
{
    [Header("UI Panel sa šifrom")]
    public GameObject codePanel;

    protected override void OnInteract()
    {
        if (codePanel != null)
            codePanel.SetActive(true);


        Light hintLight = GetComponentInChildren<Light>();
        if (hintLight != null)
            hintLight.gameObject.SetActive(false);
    }
}