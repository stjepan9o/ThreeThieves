using UnityEngine;
using System.Collections;

/// <summary>
/// Sci-Fi vrata koja se KLIZE (ne rotiraju kao Door).
/// Svaki panel ima svoj smjer i udaljenost klizanja, pa radi i s
/// nesimetricnim modelima (npr. MASH gate gdje krila nisu jednaka).
///
/// Podesavanje smjera BEZ Play-a:
///   Desni klik na "Sci Fi Gate" komponentu u Inspectoru ->
///     - "Capture Closed (trenutna poza)"  = zapamti trenutni polozaj kao ZATVORENO
///     - "Preview: Open"  = pomakni krila u otvoreno (da vidis je li smjer/udaljenost dobra)
///     - "Preview: Closed" = vrati na zatvoreno
///   Ugadjaj Direction/Distance dok "Preview: Open" ne izgleda dobro.
///
/// Za pathfinding: krila moraju biti na "Wall" layeru dok su zatvorena
/// (SciFiGateSetup alat to postavi). Pri otvaranju skripta oslobodi layer
/// i pozove GridManager.RebuildGrid().
/// </summary>
public class SciFiGate : InteractableObject
{
    public enum SlideDirection { Up, Down, Left, Right, Forward, Back }

    [Header("Panels")]
    public Transform panelA;
    [Tooltip("Drugi panel za split vrata. Ostavi prazno za jednopanelna.")]
    public Transform panelB;

    [Header("Panel A - klizanje")]
    public SlideDirection directionA = SlideDirection.Left;
    public float distanceA = 3f;

    [Header("Panel B - klizanje (split vrata)")]
    [Tooltip("Ako je ukljuceno, panel B ide automatski suprotno od panela A.")]
    public bool panelBMirrorsA = true;
    public SlideDirection directionB = SlideDirection.Right;
    public float distanceB = 3f;

    [Header("Animacija")]
    public float slideSpeed = 3f;

    [Header("Character Restriction")]
    public CharacterType requiredCharacter = CharacterType.None;

    [Header("Ponasanje")]
    [Tooltip("Ako je ukljuceno, ponovna interakcija zatvara vrata.")]
    public bool canClose = true;
    [Tooltip("Sekunde nakon kojih se vrata sama zatvore. 0 = nikad.")]
    public float autoCloseDelay = 0f;

    [Header("Audio (opcija)")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    // Zapamceni zatvoreni polozaji (izvor istine za animaciju i preview).
    [SerializeField, HideInInspector] private Vector3 closedLocalA;
    [SerializeField, HideInInspector] private Vector3 closedLocalB;
    [SerializeField, HideInInspector] private bool captured;

    private int closedLayerA, closedLayerB;
    private const int openLayer = 0; // "Default"

    protected bool isOpen = false;
    protected bool isAnimating = false;

    Vector3 OffsetA => Dir(directionA) * distanceA;
    Vector3 OffsetB => panelBMirrorsA ? -OffsetA : Dir(directionB) * distanceB;

    void Start()
    {
        interactionPrompt = "Otvori vrata";
        apCost = 2;

        // Ako nismo rucno zapamtili zatvoreno, uzmi trenutni polozaj kao zatvoreno.
        if (!captured)
        {
            if (panelA != null) closedLocalA = panelA.localPosition;
            if (panelB != null) closedLocalB = panelB.localPosition;
            captured = true;
        }

        // Snapaj na zatvoreno na startu (da preview poza u editoru ne pokvari pocetak).
        if (panelA != null) { panelA.localPosition = closedLocalA; closedLayerA = panelA.gameObject.layer; }
        if (panelB != null) { panelB.localPosition = closedLocalB; closedLayerB = panelB.gameObject.layer; }
    }

    protected override void OnInteract()
    {
        if (isAnimating) return;

        if (isOpen)
        {
            if (canClose) Toggle();
            return;
        }

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
                Debug.Log($"Ova vrata moze otvoriti samo {requiredCharacter}! Trenutno aktivan: {activeType}");
                return;
            }
        }

        ForceOpen();
    }

    /// <summary>Otvori vrata bez provjere lika (keypad, poluga, hacker...).</summary>
    public void ForceOpen()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(Animate(true));
    }

    /// <summary>Zatvori vrata izvana.</summary>
    public void ForceClose()
    {
        if (!isOpen || isAnimating) return;
        StartCoroutine(Animate(false));
    }

    /// <summary>Prebaci stanje (otvoreno &lt;-&gt; zatvoreno).</summary>
    public void Toggle()
    {
        if (isAnimating) return;
        StartCoroutine(Animate(!isOpen));
    }

    IEnumerator Animate(bool opening)
    {
        isAnimating = true;
        Debug.Log($"{gameObject.name}: Vrata se {(opening ? "otvaraju" : "zatvaraju")}!");
        PlaySound(opening ? openSound : closeSound);

        // Pri otvaranju odmah oslobodi layer (RebuildGrid na kraju vidi prolaz slobodnim).
        if (opening) SetPanelLayers(openLayer);

        Vector3 fromA = panelA != null ? panelA.localPosition : Vector3.zero;
        Vector3 toA = opening ? closedLocalA + OffsetA : closedLocalA;
        Vector3 fromB = panelB != null ? panelB.localPosition : Vector3.zero;
        Vector3 toB = opening ? closedLocalB + OffsetB : closedLocalB;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            if (panelA != null) panelA.localPosition = Vector3.LerpUnclamped(fromA, toA, s);
            if (panelB != null) panelB.localPosition = Vector3.LerpUnclamped(fromB, toB, s);
            yield return null;
        }

        if (panelA != null) panelA.localPosition = toA;
        if (panelB != null) panelB.localPosition = toB;

        if (!opening) RestorePanelLayers();

        isOpen = opening;
        isAnimating = false;

        if (GridManager.Instance != null)
            GridManager.Instance.RebuildGrid();

        Debug.Log($"{gameObject.name}: Vrata su {(opening ? "otvorena" : "zatvorena")}!");

        if (opening && canClose && autoCloseDelay > 0f)
            StartCoroutine(AutoClose());
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        ForceClose();
    }

    void SetPanelLayers(int layer)
    {
        if (panelA != null) panelA.gameObject.layer = layer;
        if (panelB != null) panelB.gameObject.layer = layer;
    }

    void RestorePanelLayers()
    {
        if (panelA != null) panelA.gameObject.layer = closedLayerA;
        if (panelB != null) panelB.gameObject.layer = closedLayerB;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
        else if (CharacterSwitcher.Instance != null && CharacterSwitcher.Instance.audioSource != null)
            CharacterSwitcher.Instance.audioSource.PlayOneShot(clip);
    }

    static Vector3 Dir(SlideDirection d)
    {
        switch (d)
        {
            case SlideDirection.Up: return Vector3.up;
            case SlideDirection.Down: return Vector3.down;
            case SlideDirection.Left: return Vector3.left;
            case SlideDirection.Right: return Vector3.right;
            case SlideDirection.Forward: return Vector3.forward;
            case SlideDirection.Back: return Vector3.back;
            default: return Vector3.left;
        }
    }

    // ---------- Editor preview (radi bez Play-a) ----------

    [ContextMenu("Capture Closed (trenutna poza)")]
    void CaptureClosed()
    {
        if (panelA != null) closedLocalA = panelA.localPosition;
        if (panelB != null) closedLocalB = panelB.localPosition;
        captured = true;
        Debug.Log($"{name}: zapamcen zatvoreni polozaj krila.");
    }

    [ContextMenu("Preview: Open")]
    void PreviewOpen()
    {
        if (!captured) CaptureClosed();
        if (panelA != null) panelA.localPosition = closedLocalA + OffsetA;
        if (panelB != null) panelB.localPosition = closedLocalB + OffsetB;
    }

    [ContextMenu("Preview: Closed")]
    void PreviewClosed()
    {
        if (!captured) return;
        if (panelA != null) panelA.localPosition = closedLocalA;
        if (panelB != null) panelB.localPosition = closedLocalB;
    }
}
