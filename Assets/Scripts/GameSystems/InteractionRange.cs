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

    /// <summary>
    /// Kao gornja, ali mjeri do NAJBLIZE tocke collidera (Collider.ClosestPoint)
    /// umjesto do pivota. Vazno za velike/siroke objekte (vrata): lik dohoda na
    /// susjedno polje koje je dijagonalno ~2.83 od pivota pa provjera na 2.5 padne,
    /// iako lik fizicki stoji uz vrata. Ako collider fali, pada natrag na pivot.
    /// </summary>
    public static bool IsActiveCharacterInRange(Vector3 targetPosition, Collider targetCollider, float range)
    {
        if (CharacterSwitcher.Instance == null)
            return true;

        GridPlayerController active = CharacterSwitcher.Instance.GetActiveCharacter();
        if (active == null)
            return true;

        Vector3 a = active.transform.position;
        Vector3 b = targetCollider != null ? targetCollider.ClosestPoint(a) : targetPosition;
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b) <= range;
    }
}
