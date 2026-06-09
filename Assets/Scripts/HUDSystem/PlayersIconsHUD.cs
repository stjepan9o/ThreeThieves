using UnityEngine;

public class PlayerIconsHUD : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    [SerializeField] private RectTransform infiltratorIcon;
    [SerializeField] private RectTransform muscleManIcon;
    [SerializeField] private RectTransform hackerIcon;

    public void SetActivePlayer(CharacterType type)
    {
        RectTransform target = null;

        switch (type)
        {
            case CharacterType.Infiltrator:
                target = infiltratorIcon;
                break;
            case CharacterType.MuscleMan:
                target = muscleManIcon;
                break;
            case CharacterType.Hacker:
                target = hackerIcon;
                break;
        }

        if (target != null)
            selector.anchoredPosition = target.anchoredPosition;
    }
}