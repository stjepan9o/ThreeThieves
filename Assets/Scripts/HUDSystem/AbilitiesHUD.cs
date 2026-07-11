using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesHUD : MonoBehaviour
{
    [Header("Player abilities pill")]
    [SerializeField] private Image abilityLabel;
    [SerializeField] private Sprite infiltratorLabel;
    [SerializeField] private Sprite muscleLabel;
    [SerializeField] private Sprite hackerLabel;

    [Header("Ability(G)")]
    [SerializeField] private Image stayIcon;
    [SerializeField] private Sprite infiltratorStay;
    [SerializeField] private Sprite muscleStay;
    [SerializeField] private Sprite hackerStay;

    [Header("Ability selector flash")]
    [SerializeField] private RectTransform selector;
    [SerializeField] private Image flashIcon;
    [SerializeField] private float flashDuration = 0.35f;

    private Coroutine flashRoutine;

    void Start()
    {
        if (selector != null)
            selector.gameObject.SetActive(false);
    }

    public void SetActivePlayer(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Infiltrator:
                abilityLabel.sprite = infiltratorLabel;
                stayIcon.sprite = infiltratorStay;
                break;
            case CharacterType.MuscleMan:
                abilityLabel.sprite = muscleLabel;
                stayIcon.sprite = muscleStay;
                break;
            case CharacterType.Hacker:
                abilityLabel.sprite = hackerLabel;
                stayIcon.sprite = hackerStay;
                break;
        }


        if (flashIcon != null)
            flashIcon.gameObject.SetActive(type != CharacterType.Hacker);
    }

    public void FlashAbilityIcon(KeyCode key)
    {
        if (key == KeyCode.G && stayIcon != null)
            Flash(stayIcon.rectTransform);
    }


    public void FlashDoorAbilityIcon()
    {
        if (flashIcon != null && flashIcon.gameObject.activeSelf)
            Flash(flashIcon.rectTransform);
    }

    private void Flash(RectTransform target)
    {
        if (selector == null || target == null) return;

        selector.anchoredPosition = target.anchoredPosition;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        selector.gameObject.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        selector.gameObject.SetActive(false);
        flashRoutine = null;
    }
}
