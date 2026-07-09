using System.Collections;
using UnityEngine;

public class SurveillanceCameraZone : MonoBehaviour
{
    private Coroutine alarmRoutine;
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InfiltratorAbility abilities = other.GetComponent<InfiltratorAbility>();
        bool isHidden = abilities != null && abilities.isHidden;

        if (!isHidden && alarmRoutine == null)
        {
            alarmRoutine = StartCoroutine(AlarmTick());
        }

        else if (isHidden && alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }


    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }
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