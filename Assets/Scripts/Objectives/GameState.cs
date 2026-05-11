using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public bool hasKeycard = false;
    public bool missionComplete = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PickupKeycard()
    {
        hasKeycard = true;
        Debug.Log("Keycard pokupljena!");
    }

    public void CompleteMission()
    {
        missionComplete = true;
        Debug.Log("Misija završena!");
    }
}