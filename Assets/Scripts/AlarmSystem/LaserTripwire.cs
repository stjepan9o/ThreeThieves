using UnityEngine;

public class LaserTripwire : MonoBehaviour
{
    [Header("Tripwire Settings")]
    public float retriggerCooldown = 1f;

    private float lastTriggerTime = -999f;

    void OnTriggerEnter(Collider other)
    {


        if (!other.CompareTag("Player")) return;


        if (Time.time - lastTriggerTime < retriggerCooldown) return;
        lastTriggerTime = Time.time;

        AlarmSystem.Instance.IncreaseAlarm();

    }
}
