using UnityEngine;
using System.Collections;

public class VaultInteraction : MonoBehaviour
{
    public Transform vaultDoor;

    [Header("Victory Settings")]
    public float victoryDelay = 2.5f;

    [Header("Audio")]
    public AudioClip openSound;

    private const float OpenSoundDelay = 1.5f;

    private bool isOpening = false;
    private bool isOpened = false;
    private Quaternion targetRotation = Quaternion.Euler(-90f, -100f, 0f);

    public bool IsOpened => isOpened;

    private void Awake()
    {
        if (openSound == null)
            openSound = Resources.Load<AudioClip>("vault-opening");
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

    public bool TryForceOpen()
    {
        if (isOpened || isOpening) return false;

        if (GameState.Instance == null || !GameState.Instance.hasKeycard)
            return false;

        Debug.Log("Sef otvoren!");
        isOpened = true;
        isOpening = true;

        StartCoroutine(PlayOpenSoundDelayed());

        return true;
    }

    IEnumerator VictoryRoutine()
    {
        yield return new WaitForSeconds(victoryDelay);
        GameOverManager.Instance?.TriggerVictory();
    }

    IEnumerator PlayOpenSoundDelayed()
    {
        yield return new WaitForSeconds(OpenSoundDelay);

        if (openSound != null && CharacterSwitcher.Instance != null &&
            CharacterSwitcher.Instance.audioSource != null)
        {
            CharacterSwitcher.Instance.audioSource.PlayOneShot(openSound, 2f);
        }
    }
}
