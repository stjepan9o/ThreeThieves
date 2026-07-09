using UnityEngine;
using UnityEngine.UI;

public class AbilitiesHUD : MonoBehaviour
{
    [Header("Player abilities pill")]
    [SerializeField] private Image abilityLabel;
    [SerializeField] private Sprite infiltratorLabel;
    [SerializeField] private Sprite muscleLabel;
    [SerializeField] private Sprite hackerLabel;

    [Header("Ability(H)")]
    [SerializeField] private Image stayIcon;
    [SerializeField] private Sprite infiltratorStay;
    [SerializeField] private Sprite muscleStay;
    [SerializeField] private Sprite hackerStay;

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
        
    }
}