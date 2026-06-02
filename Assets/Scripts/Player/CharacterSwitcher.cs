using UnityEngine;

// Postavi ovu skriptu na GameManager objekt
// Upravlja prebacivanjem između 3 lika
public class CharacterSwitcher : MonoBehaviour
{
    [Header("Characters")]
    public GridPlayerController infiltrator;  // Tipka 1
    public GridPlayerController musclMan;     // Tipka 2
    public GridPlayerController hacker;       // Tipka 3 - ne moze se kretati

    [Header("Camera")]
    public CameraFollow cameraFollow; // Povuci Main Camera ovdje

    private GridPlayerController activeCharacter;

    void Start()
    {
        // Postavi Hacker da se ne moze kretati
        if (hacker != null)
            hacker.canMove = false;

        // Auto-pronadji kameru ako nije assignana
        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        // Pocni s Infiltratorom
        SwitchTo(infiltrator);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchTo(infiltrator);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchTo(musclMan);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchTo(hacker);
    }

    void SwitchTo(GridPlayerController character)
    {
        if (character == null) return;

        // Disable svi likovi
        if (infiltrator != null) infiltrator.enabled = false;
        if (musclMan != null) musclMan.enabled = false;
        if (hacker != null) hacker.enabled = false;

        // Enable odabrani lik
        character.enabled = true;
        activeCharacter = character;

        // Kamera prati aktivnog lika
        if (cameraFollow != null)
            cameraFollow.target = character.transform;

        Debug.Log($"Aktivan lik: {character.gameObject.name}");
    }

    public GridPlayerController GetActiveCharacter()
    {
        return activeCharacter;
    }
}
