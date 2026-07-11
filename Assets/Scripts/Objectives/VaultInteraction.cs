using UnityEngine;

public class VaultInteraction : MonoBehaviour
{
    public Transform vaultDoor;
    public GameObject missionCompleteUI;
    public GameObject keycardWarningUI;

    [Header("Interaction Range")]
    [Tooltip("Maksimalna udaljenost (u poljima/jedinicama) s koje aktivni lik moze otvoriti sef.")]
    public float interactionRange = 2.5f;

    private bool isOpening = false;
    private Quaternion targetRotation = Quaternion.Euler(-90f, -100f, 0f);

    private void Start()
    {
        if (missionCompleteUI == null)
            missionCompleteUI = GameObject.Find("MissionCompleteUI");
        if (keycardWarningUI == null)
            keycardWarningUI = GameObject.Find("KeycardWarning");
    }

    private void Update()
    {
        if (isOpening && vaultDoor != null)
        {
            vaultDoor.localRotation = Quaternion.RotateTowards(
                vaultDoor.localRotation,
                targetRotation,
                30f * Time.deltaTime
            );

            if (vaultDoor.localRotation == targetRotation)
                isOpening = false;
        }
    }

    private void OnMouseDown()
    {
        GridPlayerController active = CharacterSwitcher.Instance != null
            ? CharacterSwitcher.Instance.GetActiveCharacter()
            : null;

        if (active != null)
        {
            // Lik ce sam dohodati do sefa (ako je predaleko) i otvoriti ga kad stigne.
            active.RequestInteraction(transform.position, interactionRange, OpenVault);
            return;
        }

        OpenVault();
    }

    private void OpenVault()
    {
        if (GameState.Instance.hasKeycard)
        {
            Debug.Log("MISSION COMPLETE");
            GameState.Instance.CompleteMission();
            isOpening = true;
            if (missionCompleteUI != null)
                missionCompleteUI.SetActive(true);
        }
        else
        {
            if (keycardWarningUI != null)
            {
                keycardWarningUI.SetActive(true);
                Invoke(nameof(HideWarning), 2f);
            }
        }
    }

    private void HideWarning()
    {
        if (keycardWarningUI != null)
            keycardWarningUI.SetActive(false);
    }
}