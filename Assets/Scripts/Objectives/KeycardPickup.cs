using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    [Header("Interaction Range")]
    [Tooltip("Maksimalna udaljenost (u poljima/jedinicama) s koje aktivni lik moze pokupiti karticu.")]
    public float interactionRange = 2.5f;

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
        Destroy(gameObject);
    }
}
