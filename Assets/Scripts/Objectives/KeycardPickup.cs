using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    [Header("Interaction Range")]
    [Tooltip("Maksimalna udaljenost (u poljima/jedinicama) s koje aktivni lik moze pokupiti karticu.")]
    public float interactionRange = 2.5f;

    [Header("Audio")]
    public AudioClip pickupSound;

    private void OnMouseDown()
    {
        GridPlayerController active = CharacterSwitcher.Instance != null
            ? CharacterSwitcher.Instance.GetActiveCharacter()
            : null;

        if (active != null)
        {
            // Lik ce sam dohodati do kartice (ako je predaleko) i pokupiti je kad stigne.
            active.RequestInteraction(transform.position, interactionRange, Pickup);
            return;
        }

        Pickup();
    }

    private void Pickup()
    {
        GameState.Instance.PickupKeycard();

        if (pickupSound != null && CharacterSwitcher.Instance != null &&
            CharacterSwitcher.Instance.audioSource != null)
        {
            CharacterSwitcher.Instance.audioSource.PlayOneShot(pickupSound);
        }

        Destroy(gameObject);
    }
}
