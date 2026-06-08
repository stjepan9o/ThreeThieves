using UnityEngine;
using System;

public class AlarmSystem : MonoBehaviour
{
  public static AlarmSystem Instance { get; private set; }
  public enum AlarmState { Empty, Yellow, Orange, Red }
  public int alarmLevel = 0;
  public int maxAlarmLevel = 3;
  private AlarmState currentState = AlarmState.Empty;
  public event Action<AlarmState> OnStateChanged;

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
    
  }

  public void DecreaseAlarm()
  {
    alarmLevel = Mathf.Max(alarmLevel - 1, 0);
    UpdateState();
    
  }

  private void UpdateState()
  {
    AlarmState newState = GetAlarmState();
    if(currentState != newState)
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
