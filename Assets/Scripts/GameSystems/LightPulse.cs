using UnityEngine;


[RequireComponent(typeof(Light))]
public class LightPulse : MonoBehaviour
{
    public float minIntensity = 1.5f;
    public float maxIntensity = 4f;
    public float pulseSpeed = 2f;

    private Light pulseLight;

    void Awake()
    {
        pulseLight = GetComponent<Light>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
        pulseLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
