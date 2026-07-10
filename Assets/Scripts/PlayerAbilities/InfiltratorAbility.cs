using UnityEngine;

// TO DO: povezati do kraja abilitye infiltratora, ap system
public class InfiltratorAbility : MonoBehaviour
{
    [Header("Ability Costs")]
    public int hideCost = 3;

    public bool isHidden = false;
    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (CharacterSwitcher.Instance == null ||
            CharacterSwitcher.Instance.ActiveCharacterType != CharacterType.Infiltrator)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isHidden) DeactivateHide();
            else ActivateHide();

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