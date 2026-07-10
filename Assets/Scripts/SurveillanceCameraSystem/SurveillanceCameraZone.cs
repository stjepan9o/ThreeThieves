using System.Collections;
using UnityEngine;

public class SurveillanceCameraZone : MonoBehaviour
{
    [Header("Hack")]
    public int cameraNumber;
    public bool isHacked = false;

    private Coroutine alarmRoutine;
    private Coroutine hackRoutine;

    void Start()
    {
        if (CameraHackSystem.Instance != null)
            CameraHackSystem.Instance.OnCameraHacked += HandleCameraHacked;
    }

    void OnDisable()
    {
        if (CameraHackSystem.Instance != null)
            CameraHackSystem.Instance.OnCameraHacked -= HandleCameraHacked;
    }

    private void HandleCameraHacked(int hackedNumber)
    {
        if (hackedNumber != cameraNumber) return;


        if (alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }

        if (hackRoutine != null) StopCoroutine(hackRoutine);
        hackRoutine = StartCoroutine(HackTimeout());
    }

    private IEnumerator HackTimeout()
    {
        isHacked = true;
        yield return new WaitForSeconds(CameraHackSystem.Instance.hackDuration);
        isHacked = false;
        hackRoutine = null;
    }

    private bool visiblePlayerInside;

    void OnTriggerStay(Collider other)
    {
        if (isHacked) return;

        if (!other.CompareTag("Player")) return;

        InfiltratorAbility abilities = other.GetComponent<InfiltratorAbility>();
        bool isHidden = abilities != null && abilities.isHidden;

        if (!isHidden)
            visiblePlayerInside = true;
    }

    void FixedUpdate()
    {
        if (visiblePlayerInside && alarmRoutine == null)
        {
            alarmRoutine = StartCoroutine(AlarmTick());
        }
        else if (!visiblePlayerInside && alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }

        visiblePlayerInside = false;
    }

    private IEnumerator AlarmTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            AlarmSystem.Instance.IncreaseAlarm();
        }
    }
}
