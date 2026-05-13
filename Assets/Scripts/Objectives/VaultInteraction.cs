using UnityEngine;

public class VaultInteraction : MonoBehaviour
{
    public Transform vaultDoor;
    public GameObject missionCompleteUI;
    public GameObject keycardWarningUI;

    private bool isOpening = false;
    private Quaternion targetRotation = Quaternion.Euler(-90f, -100f, 0f);

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
        if (GameState.Instance.hasKeycard)
        {
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