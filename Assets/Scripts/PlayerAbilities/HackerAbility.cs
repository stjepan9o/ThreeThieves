using UnityEngine;

public class HackerAbility : MonoBehaviour
{
    [Header("Ability Costs")]
    public int hackCost = 2;

    private ActionPointManager apManager;
    private HackerCameraController cameraController;

    void Start()
    {
        apManager = FindFirstObjectByType<ActionPointManager>();
        cameraController = FindFirstObjectByType<HackerCameraController>();
    }

    void Update()
    {
        if (CharacterSwitcher.Instance == null ||
            CharacterSwitcher.Instance.ActiveCharacterType != CharacterType.Hacker)
            return;

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn)
            return;

        if (HackMinigame.Instance == null || HackMinigame.Instance.IsRunning)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (CharacterSwitcher.Instance.abilitiesHUD != null)
                CharacterSwitcher.Instance.abilitiesHUD.FlashAbilityIcon(KeyCode.G);

            TryHack();
        }
    }

    private void TryHack()
    {
        if (apManager == null || cameraController == null || CameraHackSystem.Instance == null)
        {
            Debug.LogError("HackerAbility: fali ActionPointManager, HackerCameraController ili CameraHackSystem u sceni!");
            return;
        }

        int cameraNumber = cameraController.GetCurrentCameraNumber();
        if (cameraNumber < 0) return;

        if (CameraHackSystem.Instance.IsHacked(cameraNumber))
        {
            Debug.Log($"Kamera {cameraNumber} vec hakirana!");
            return;
        }

        if (!apManager.HasEnoughAP(hackCost))
        {
            apManager.NotifyNotEnoughAP();
            return;
        }

        HackMinigame.Instance.StartGame(success =>
        {

            apManager.SpendAP(hackCost);

            if (success)
                CameraHackSystem.Instance.HackCamera(cameraNumber);
        });
    }
}
