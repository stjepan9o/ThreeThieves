using UnityEngine;

public class InteractableBook : MonoBehaviour
{
    public const float InteractionRange = 6f;

    [Header("UI Panel sa šifrom")]
    public GameObject codePanel;

    [Header("Audio")]
    public AudioClip pickupSound;

    private void OnMouseDown()
    {
        GridPlayerController active = CharacterSwitcher.Instance != null
            ? CharacterSwitcher.Instance.GetActiveCharacter()
            : null;

        if (active != null)
        {
            active.RequestInteraction(transform.position, InteractionRange, Open);
            return;
        }

        Open();
    }

    public void Open()
    {
        if (codePanel != null)
            codePanel.SetActive(true);

        if (pickupSound != null && CharacterSwitcher.Instance != null &&
            CharacterSwitcher.Instance.audioSource != null)
        {
            CharacterSwitcher.Instance.audioSource.PlayOneShot(pickupSound);
        }

        Light hintLight = GetComponentInChildren<Light>();
        if (hintLight != null)
            hintLight.gameObject.SetActive(false);
    }
}
