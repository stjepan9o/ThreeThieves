using UnityEngine;
using UnityEngine.UI;

public class AlarmHUDSubscriber : MonoBehaviour
{
  [SerializeField] private Image barImage;
  [SerializeField] private Sprite barEmpty;
  [SerializeField] private Sprite barYellow;
  [SerializeField] private Sprite barOrange;
  [SerializeField] private Sprite barRed;

  private void Start()
  {
    AlarmSystem.Instance.OnStateChanged += UpdateBar;
    UpdateBar(AlarmSystem.Instance.GetAlarmState());
  }

  private void OnDisable()
  {
    if (AlarmSystem.Instance != null)
      AlarmSystem.Instance.OnStateChanged -= UpdateBar;
  }

  private void UpdateBar(AlarmSystem.AlarmState state)
  {
    switch (state)
    {
      case AlarmSystem.AlarmState.Empty:  barImage.sprite = barEmpty;  break;
      case AlarmSystem.AlarmState.Yellow: barImage.sprite = barYellow; break;
      case AlarmSystem.AlarmState.Orange: barImage.sprite = barOrange; break;
      case AlarmSystem.AlarmState.Red:    barImage.sprite = barRed;    break;
    }
  }
}