using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
  public static AlarmSystem Instance { get; private set; }
  public enum AlarmState { Green, Yellow, Red }

  public int alarmLevel = 0;
  public int maxAlarmLevel = 3;

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
    Debug.Log("Alarm level: " + GetAlarmState());
  }

  public void DecreaseAlarm()
  {
    alarmLevel = Mathf.Max(alarmLevel - 1, 0);
    Debug.Log("Alarm level: " + GetAlarmState());
  }


  public AlarmState GetAlarmState()
  {
    if (alarmLevel == 3) return AlarmState.Red;
    if (alarmLevel == 2) return AlarmState.Yellow;
    return AlarmState.Green;
  }
}