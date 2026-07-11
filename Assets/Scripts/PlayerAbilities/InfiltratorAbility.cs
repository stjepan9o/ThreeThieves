using UnityEngine;

public class InfiltratorAbility : MonoBehaviour
{
    [Header("Ability Costs")]
    public int hideCost = 2;

    public bool isHidden = false;
    private Renderer[] renderers;
    private ActionPointManager apManager;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        apManager = FindFirstObjectByType<ActionPointManager>();
    }

    void OnEnable()
    {
        TurnManager.OnPlayerTurnStart += ResetHide;
    }

    void OnDisable()
    {
        TurnManager.OnPlayerTurnStart -= ResetHide;
    }


    private void ResetHide()
    {
        if (isHidden)
            DeactivateHide();
    }

    void Update()
    {
        if (CharacterSwitcher.Instance == null ||
            CharacterSwitcher.Instance.ActiveCharacterType != CharacterType.Infiltrator)
            return;

        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {

            if (isHidden) return;

            if (apManager != null && !apManager.HasEnoughAP(hideCost))
            {
                apManager.NotifyNotEnoughAP();
                return;
            }

            ActivateHide();

            if (apManager != null)
                apManager.SpendAP(hideCost);

            if (CharacterSwitcher.Instance.abilitiesHUD != null)
                CharacterSwitcher.Instance.abilitiesHUD.FlashAbilityIcon(KeyCode.G);
        }
    }

    public void ActivateHide()
    {
        isHidden = true;
        SetOpacity(0.4f);
    }

    public void DeactivateHide()
    {
        isHidden = false;
        SetOpacity(1f);
    }

    private void SetOpacity(float alpha)
    {
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}
