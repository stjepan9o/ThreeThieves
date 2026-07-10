using UnityEngine;
using TMPro;


public class UIBlink : MonoBehaviour
{
    public float blinkInterval = 0.6f;

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (text == null) return;

        bool visible = Mathf.Repeat(Time.time, blinkInterval * 2f) < blinkInterval;
        text.alpha = visible ? 1f : 0.15f;
    }
}
