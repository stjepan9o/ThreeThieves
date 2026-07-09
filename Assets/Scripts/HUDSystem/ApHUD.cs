using UnityEngine;
using TMPro;

public class APDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text apText;
    [SerializeField] private ActionPointManager apManager;

    void Start()
    {
        apManager.OnAPChanged += UpdateText;
        UpdateText(apManager.CurrentAP);
    }

    void OnDestroy()
    {
        if (apManager != null)
            apManager.OnAPChanged -= UpdateText;
    }

    void UpdateText(int ap)
    {
        apText.text = "AP: " + ap;
    }
}