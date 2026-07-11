using UnityEngine;

/// <summary>
/// Pomocna klasa za provjeru je li aktivni lik dovoljno blizu objektu
/// da bi mogao interaktirati s njim. Mjeri horizontalnu (XZ) udaljenost
/// pa visina objekta (npr. kartica na stolu, visoka vrata sefa) ne smeta.
/// </summary>
public static class InteractionRange
{
    public static bool IsActiveCharacterInRange(Vector3 targetPosition, float range)
    {
        if (CharacterSwitcher.Instance == null)
        {
            // Bez CharacterSwitchera ne mozemo znati gdje je igrac -> ne blokiraj.
            return true;
        }

        GridPlayerController active = CharacterSwitcher.Instance.GetActiveCharacter();
        if (active == null)
            return true;

        Vector3 a = active.transform.position;
        Vector3 b = targetPosition;
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b) <= range;
    }
}
