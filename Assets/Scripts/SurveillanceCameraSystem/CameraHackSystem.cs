using UnityEngine;
using System;
using System.Collections.Generic;

public class CameraHackSystem : MonoBehaviour
{
  public static CameraHackSystem Instance { get; private set; }

  [Header("Hack Settings")]
  public float hackDuration = 15f;

  public event Action<int> OnCameraHacked;

  private Dictionary<int, float> hackedUntil = new Dictionary<int, float>();

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public void HackCamera(int cameraNumber)
  {
    hackedUntil[cameraNumber] = Time.time + hackDuration;
    OnCameraHacked?.Invoke(cameraNumber);
    Debug.Log($"Kamera {cameraNumber} hakirana na {hackDuration} sekundi!");
  }

  public bool IsHacked(int cameraNumber)
  {
    return hackedUntil.TryGetValue(cameraNumber, out float until) && Time.time < until;
  }
}
