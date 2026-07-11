using UnityEngine;
using System;
using System.Collections;

public class AlarmSystem : MonoBehaviour
{
  public static AlarmSystem Instance { get; private set; }
  public enum AlarmState { Empty, Yellow, Orange, Red }
  public int alarmLevel = 0;
  public int maxAlarmLevel = 3;
  private AlarmState currentState = AlarmState.Empty;
  public event Action<AlarmState> OnStateChanged;

  [Header("Audio")]
  public AudioSource audioSource;
  public AudioClip alarm1;
  public AudioClip alarm2;
  public AudioClip alarm3;
  public float alarmSoundDuration = 4f;

  private Coroutine soundStopRoutine;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public void IncreaseAlarm()
  {
    alarmLevel = Mathf.Min(alarmLevel + 1, maxAlarmLevel);
    UpdateState();
    PlayAlarmSound();
  }

  private void PlayAlarmSound()
  {
    if (audioSource == null) return;

    AudioClip clip = null;
    if (alarmLevel == 1) clip = alarm1;
    else if (alarmLevel == 2) clip = alarm2;
    else if (alarmLevel >= 3) clip = alarm3;

    if (clip == null) return;

    audioSource.Stop();
    audioSource.clip = clip;
    audioSource.Play();


    if (soundStopRoutine != null) StopCoroutine(soundStopRoutine);
    soundStopRoutine = StartCoroutine(StopSoundAfter(alarmSoundDuration));
  }

  private IEnumerator StopSoundAfter(float seconds)
  {
    yield return new WaitForSeconds(seconds);
    audioSource.Stop();
    soundStopRoutine = null;
  }

  public void DecreaseAlarm()
  {
    alarmLevel = Mathf.Max(alarmLevel - 1, 0);
    UpdateState();

  }

  private void UpdateState()
  {
    AlarmState newState = GetAlarmState();
    if (currentState != newState)
    {
      currentState = newState;
      OnStateChanged?.Invoke(currentState);
    }
  }


  public AlarmState GetAlarmState()
  {
    if (alarmLevel == 3) return AlarmState.Red;
    if (alarmLevel == 2) return AlarmState.Orange;
    if (alarmLevel == 1) return AlarmState.Yellow;
    return AlarmState.Empty;
  }
}
