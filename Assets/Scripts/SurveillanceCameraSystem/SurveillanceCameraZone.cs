using System.Collections;
using UnityEngine;

//TO DO: Define logic for alart meter cooldown for onTriggerExit or hide infiltrator ability in OnTriggerStary

public class SurveillanceCameraZone : MonoBehaviour
{
    private Coroutine alarmRoutine;
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InfiltratorAbility abilities = other.GetComponent<InfiltratorAbility>();
        if (!abilities.isHidden == null) return;

        if (!abilities.isHidden && alarmRoutine == null)
        {
            alarmRoutine = StartCoroutine(AlarmTick());
        }

        if (abilities.isHidden && alarmRoutine != null)
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
            AlarmSystem.Instance.IncreaseAlarm();
            yield return new WaitForSeconds(5f);
        }
    }
}