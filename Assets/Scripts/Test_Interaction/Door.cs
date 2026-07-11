using UnityEngine;
using System.Collections;

public class Door : InteractableObject
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool openInward = false;

    [Header("Interaction")]
    [Tooltip("Tipka kojom aktivni lik otvara vrata kad je dovoljno blizu.")]
    public KeyCode interactKey = KeyCode.H;

    [Header("Character Restriction")]
    public CharacterType requiredCharacter = CharacterType.None;

    [Header("Audio")]
    public AudioClip openSound;

    protected bool isOpen = false;
    protected bool isAnimating = false;
    public GameObject doorMesh;

    // Collider vrata za mjerenje blizine do najblize tocke (ne do pivota).
    protected Collider rangeCollider;
    protected ActionPointManager apManager;

    void Start()
    {
        interactionPrompt = "Otvori vrata (H)";
        apCost = 2;
        rangeCollider = GetComponentInChildren<Collider>();
        apManager = FindFirstObjectByType<ActionPointManager>();
    }

    void Update()
    {
        if (isOpen || isAnimating) return;
        if (!Input.GetKeyDown(interactKey)) return;

        // Ne tijekom neprijateljskog poteza ni dok je otvoren keypad/minigame.
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return;
        if (KeypadUI.IsKeypadOpen || WireTaskManager.IsMinigameOpen) return;

        if (!InteractionRange.IsActiveCharacterInRange(transform.position, rangeCollider, interactionRange))
            return;

        // Bljesni door-ikonicu kao potvrdu (sama je skrivena kad je Hacker aktivan).
        if (CharacterSwitcher.Instance != null && CharacterSwitcher.Instance.abilitiesHUD != null)
            CharacterSwitcher.Instance.abilitiesHUD.FlashDoorAbilityIcon();

        Interact();
    }

    protected override void OnInteract()
    {
        if (isOpen || isAnimating) return;

        if (!InteractionRange.IsActiveCharacterInRange(transform.position, rangeCollider, interactionRange))
        {
            Debug.Log($"{gameObject.name}: Predaleko si od vrata - pridji blize.");
            return;
        }

        // Provjeri je li pravi lik aktivan
        if (requiredCharacter != CharacterType.None)
        {
            if (CharacterSwitcher.Instance == null)
            {
                Debug.LogWarning("CharacterSwitcher nije pronadjen!");
                return;
            }

            CharacterType activeType = CharacterSwitcher.Instance.ActiveCharacterType;

            if (activeType != requiredCharacter)
            {
                APDisplay.Instance?.ShowMessage($"Only the {requiredCharacter} can open this door!");
                return;
            }
        }

        if (apManager != null)
        {
            if (!apManager.HasEnoughAP(apCost))
            {
                apManager.NotifyNotEnoughAP();
                return;
            }
            apManager.SpendAP(apCost);
        }

        Debug.Log($"{gameObject.name}: Vrata se otvaraju!");
        StartCoroutine(OpenDoor());
    }

    /// <summary>
    /// Javna metoda za otvaranje vrata izvana (npr. CodeDoor nakon tocnog koda).
    /// Preskace provjeru requiredCharacter jer je kod vec "kljuc".
    /// </summary>
    public void ForceOpen()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        isAnimating = true;

        if (openSound != null && CharacterSwitcher.Instance != null &&
            CharacterSwitcher.Instance.audioSource != null)
        {
            CharacterSwitcher.Instance.audioSource.PlayOneShot(openSound);
        }

        float targetAngle = openInward ? -openAngle : openAngle;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, targetAngle, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, Mathf.Clamp01(t));
            yield return null;
        }

        transform.rotation = endRotation;

        // Oslobodi prolaz: ugasi SVE collidere vrata (ukljucujuci one na djeci,
        // npr. DoorMesh). Mijenjanje layera samo na rootu ne pomaze ako je collider
        // na childu - zato ovako. Otvorena vrata ne trebaju koliziju.
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Default");
        isOpen = true;
        isAnimating = false;
        Debug.Log("Vrata su otvorena!");

        if (GridManager.Instance != null)
            GridManager.Instance.RebuildGrid();
    }
}
