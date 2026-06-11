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
    public GridPlayerController hacker;       // Tipka 3 

    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("HUD")]
    public PlayerIconsHUD playerIconsHUD;

    [Header("HUD")]
    public AbilitiesHUD abilitiesHUD;

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

        SwitchTo(infiltrator, CharacterType.Infiltrator);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchTo(infiltrator, CharacterType.Infiltrator);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchTo(musclMan, CharacterType.MuscleMan);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchTo(hacker, CharacterType.Hacker);
    }

    void SwitchTo(GridPlayerController character, CharacterType type)
    {
        if (character == null) return;

        if (infiltrator != null) infiltrator.enabled = false;
        if (musclMan != null) musclMan.enabled = false;
        if (hacker != null) hacker.enabled = false;

        character.enabled = true;
        activeCharacter = character;
        ActiveCharacterType = type;

        if (cameraFollow != null)
            cameraFollow.target = character.transform;

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