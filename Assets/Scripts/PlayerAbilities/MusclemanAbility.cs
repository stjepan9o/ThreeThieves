using UnityEngine;

public class MusclemanAbility : MonoBehaviour
{
    [Header("Ability Costs")]
    public int openVaultCost = 2;

    [Header("Audio")]
    public AudioClip openVaultSound;

    private const float OpenRange = 10f;

    private ActionPointManager apManager;
    private VaultInteraction vault;
    private Collider vaultCollider;

    void Start()
    {
        apManager = FindFirstObjectByType<ActionPointManager>();
        vault = FindFirstObjectByType<VaultInteraction>();

        if (vault != null)
            vaultCollider = vault.GetComponentInChildren<Collider>();
    }

    void Update()
    {
        if (CharacterSwitcher.Instance == null ||
            CharacterSwitcher.Instance.ActiveCharacterType != CharacterType.MuscleMan)
            return;

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (CharacterSwitcher.Instance.abilitiesHUD != null)
                CharacterSwitcher.Instance.abilitiesHUD.FlashAbilityIcon(KeyCode.G);

            TryOpenVault();
        }
    }

    private void TryOpenVault()
    {
        if (apManager == null || vault == null)
        {
            Debug.LogError("MusclemanAbility: fali ActionPointManager ili VaultInteraction u sceni!");
            return;
        }

        if (!IsNearVault(CharacterSwitcher.Instance.musclMan))
        {
            APDisplay.Instance?.ShowMessage("Patience, thief... find the vault first!");
            return;
        }

        if (!IsNearVault(CharacterSwitcher.Instance.infiltrator))
        {
            APDisplay.Instance?.ShowMessage("Infiltrator must be next to the vault!");
            return;
        }

        if (!apManager.HasEnoughAP(openVaultCost))
        {
            apManager.NotifyNotEnoughAP();
            return;
        }

        if (vault.TryForceOpen())
        {
            apManager.SpendAP(openVaultCost);

            if (openVaultSound != null && CharacterSwitcher.Instance.audioSource != null)
                CharacterSwitcher.Instance.audioSource.PlayOneShot(openVaultSound);
        }
        else if (!vault.IsOpened)
            APDisplay.Instance?.ShowMessage("Keycard missing!");
    }

    private bool IsNearVault(GridPlayerController character)
    {
        if (character == null) return false;

        Vector3 a = character.transform.position;

        Vector3 b = vaultCollider != null
            ? vaultCollider.ClosestPoint(a)
            : vault.transform.position;

        a.y = 0f;
        b.y = 0f;

        float dist = Vector3.Distance(a, b);

        return dist <= OpenRange;
    }
}
