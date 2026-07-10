using UnityEngine;

/// <summary>
/// Server/terminal u security roomu. Infiltrator ga klikne -> pokrene se
/// wire minigame -> kad je rijesen, Hacker se otkljuca (CharacterSwitcher.UnlockHacker).
///
/// Setup u Unity:
/// 1) Na server objekt (mora imati Collider) stavi OVU skriptu.
/// 2) Povuci WireTaskManager objekt iz Hierarchyja u wireTask polje.
/// 3) requiredCharacter ostavi Infiltrator (ili None za svakoga).
/// </summary>
public class HackTerminal : InteractableObject
{
    [Header("Terminal")]
    public WireTaskManager wireTask;
    public CharacterType requiredCharacter = CharacterType.Infiltrator;
    [Tooltip("Ako je ukljuceno, server se moze hakirati samo jednom.")]
    public bool oneTimeUse = true;

    private bool used = false;

    void Start()
    {
        interactionPrompt = "Hakiraj server";
        apCost = 2;
    }

    protected override void OnInteract()
    {
        if (used && oneTimeUse)
        {
            Debug.Log($"{gameObject.name}: Server je vec hakiran.");
            return;
        }

        if (wireTask == null)
        {
            Debug.LogWarning($"{gameObject.name}: WireTaskManager nije povucen u Inspector!");
            return;
        }

        // Samo odredjeni lik smije hakirati (npr. Infiltrator).
        if (requiredCharacter != CharacterType.None)
        {
            if (CharacterSwitcher.Instance == null)
            {
                Debug.LogWarning("CharacterSwitcher nije pronadjen!");
                return;
            }

            if (CharacterSwitcher.Instance.ActiveCharacterType != requiredCharacter)
            {
                Debug.Log($"Samo {requiredCharacter} moze hakirati ovaj server!");
                return;
            }
        }

        // Otvori minigame; OnSolved se okine kad su sve zice spojene.
        wireTask.ShowTask(OnSolved);
    }

    void OnSolved()
    {
        used = true;

        if (CharacterSwitcher.Instance != null)
            CharacterSwitcher.Instance.UnlockHacker();

        Debug.Log($"{gameObject.name}: Server hakiran! Hacker otkljucan.");
    }
}
