using UnityEngine;

public enum CharacterType
{
    None,
    Infiltrator,
    MuscleMan,
    Hacker
}

public class CharacterSwitcher : MonoBehaviour
{
    public static CharacterSwitcher Instance { get; private set; }

    [Header("Characters")]
    public GridPlayerController infiltrator;  // Tipka 1
    public GridPlayerController musclMan;     // Tipka 2
    public GridPlayerController hacker;       // Tipka 3 - ne moze se kretati

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public FogOfWarEffect fogOfWar;

    [Header("HUD")]
    public PlayerIconsHUD playerIconsHUD;
    public AbilitiesHUD abilitiesHUD;

    [Header("Lock")]
    [Tooltip("Hacker je zakljucan dok Infiltrator ne rijesi minigame na serveru.")]
    public bool hackerUnlocked = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip switchSound;

    private GridPlayerController activeCharacter;
    public CharacterType ActiveCharacterType { get; private set; } = CharacterType.None;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (hacker != null)
            hacker.canMove = false;

        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (fogOfWar == null)
            fogOfWar = Camera.main.GetComponent<FogOfWarEffect>();

        SwitchTo(infiltrator, CharacterType.Infiltrator);
    }

    void Update()
    {
        // Dok je keypad (ili bilo koji code unos) otvoren - ignoriraj prebacivanje likova
        if (KeypadUI.IsKeypadOpen) return;
        if (WireTaskManager.IsMinigameOpen) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchTo(infiltrator, CharacterType.Infiltrator);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchTo(musclMan, CharacterType.MuscleMan);

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (hackerUnlocked)
                SwitchTo(hacker, CharacterType.Hacker);
            else
                Debug.Log("Hacker je zakljucan! Infiltrator mora hakirati server.");
        }
    }

    /// <summary>Otkljucava hackera (poziva HackTerminal kad se minigame rijesi).</summary>
    public void UnlockHacker()
    {
        hackerUnlocked = true;
        Debug.Log("Hacker je otkljucan!");
    }

    void SwitchTo(GridPlayerController character, CharacterType type)
    {
        if (character == null) return;

        if (audioSource != null && switchSound != null &&
            ActiveCharacterType != CharacterType.None && ActiveCharacterType != type)
        {
            audioSource.PlayOneShot(switchSound);
        }

        if (infiltrator != null) infiltrator.enabled = false;
        if (musclMan != null) musclMan.enabled = false;
        if (hacker != null) hacker.enabled = false;

        character.enabled = true;
        activeCharacter = character;
        ActiveCharacterType = type;

        if (cameraFollow != null)
            cameraFollow.target = character.transform;

        if (fogOfWar != null)
            fogOfWar.SetActivePlayer(character.transform);

        if (playerIconsHUD != null)
            playerIconsHUD.SetActivePlayer(type);

        if (abilitiesHUD != null)
            abilitiesHUD.SetActivePlayer(type);

        Debug.Log($"Aktivan lik: {character.gameObject.name} ({type})");
    }

    public GridPlayerController GetActiveCharacter()
    {
        return activeCharacter;
    }
}