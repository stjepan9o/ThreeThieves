using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (isHidden) DeactivateHide();
            else ActivateHide();
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