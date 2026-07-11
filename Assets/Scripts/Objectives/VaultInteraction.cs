using UnityEngine;
using System.Collections;

public class VaultInteraction : MonoBehaviour
{
    public Transform vaultDoor;
    public GameObject keycardWarningUI;

    [Header("Victory Settings")]
    [Tooltip("Koliko sekundi nakon otvaranja sefa da se pojavi end screen.")]
    public float victoryDelay = 2.5f;

    private bool isOpening = false;
    private Quaternion targetRotation = Quaternion.Euler(-90f, -100f, 0f);

    private void Start()
    {
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
            {
                isOpening = false;
                StartCoroutine(VictoryRoutine());
            }
        }
    }

    private void OnMouseDown()
    {
        if (GameState.Instance.hasKeycard)
        {
            Debug.Log("Sef otvoren!");
            GameState.Instance.CompleteMission();
            isOpening = true;
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

    IEnumerator VictoryRoutine()
    {
        yield return new WaitForSeconds(victoryDelay);
        GameOverManager.Instance?.TriggerVictory();
    }

    private void HideWarning()
    {
        if (keycardWarningUI != null)
            keycardWarningUI.SetActive(false);
    }
}
