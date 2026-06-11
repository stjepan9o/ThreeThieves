using UnityEngine;
using System;

public enum FOVState
{
    Unaware,    // Zeleno
    Suspicious, // Narancasto
    Alert       // Crveno
}
public class GuardFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    public float fovAngle = 60f;
    public float fovRange = 6f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Orientation Fix")]
    [Tooltip("Postavi na isti broj kao modelForwardOffset u UnitGridMovement. " +
             "Ispravlja konus ako je okrenut bocno.")]
    public float fovForwardOffset = 0f;

    [Header("Cone Visualization")]
    [Tooltip("Broj tocaka za crtanje luka konusa - vise = gladi luk.")]
    public int coneResolution = 12;
    [Tooltip("Visinska pozicija linije konusa iznad tla.")]
    public float coneHeight = 0.1f;

    public FOVState CurrentState { get; private set; } = FOVState.Unaware;
    public Transform DetectedPlayer { get; private set; }
    public event Action<FOVState> OnFOVStateChanged;

    private LineRenderer coneRenderer;

    private Coroutine alarmRoutine;

    void Awake()
    {
        SetupLineRenderer();
    }

    void Start()
    {

        if (AlarmSystem.Instance != null)
            AlarmSystem.Instance.OnStateChanged += OnAlarmStateChanged;
    }

    void OnDestroy()
    {
        if (AlarmSystem.Instance != null)
            AlarmSystem.Instance.OnStateChanged -= OnAlarmStateChanged;
    }

    void OnAlarmStateChanged(AlarmSystem.AlarmState state)
    {
        if (state == AlarmSystem.AlarmState.Red)
            TriggerAlert();
        else if (state == AlarmSystem.AlarmState.Empty)
            ResetToUnaware();
    }

    void SetupLineRenderer()
    {
        coneRenderer = gameObject.AddComponent<LineRenderer>();
        coneRenderer.useWorldSpace = true;
        coneRenderer.loop = true;
        coneRenderer.startWidth = 0.05f;
        coneRenderer.endWidth = 0.05f;
        coneRenderer.positionCount = coneResolution + 2; 
        coneRenderer.material = new Material(Shader.Find("Sprites/Default"));
        coneRenderer.sortingOrder = 1;
        UpdateConeColor();
    }

    void Update()
    {
        CheckFOV();
        DrawCone();
    }

    Vector3 GetForward()
    {

        return Quaternion.Euler(0f, fovForwardOffset, 0f) * transform.forward;
    }

    void CheckFOV()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, fovRange, playerMask);
        Transform detected = null;

        foreach (Collider playerCol in playersInRange)
        {
            Vector3 dirToPlayer = (playerCol.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(GetForward(), dirToPlayer);

            if (angle > fovAngle / 2f) continue;

            float dist = Vector3.Distance(transform.position, playerCol.transform.position);
            if (Physics.Raycast(transform.position, dirToPlayer, dist, obstacleMask)) continue;

            detected = playerCol.transform;
            break;
        }

        DetectedPlayer = detected;
        SetState(detected != null ? FOVState.Suspicious : FOVState.Unaware);
    }

    void DrawCone()
    {
        if (coneRenderer == null) return;

        Vector3 origin = transform.position + Vector3.up * coneHeight;
        Vector3 forward = GetForward();

        coneRenderer.SetPosition(0, origin);

        for (int i = 0; i < coneResolution; i++)
        {
            float t = (float)i / (coneResolution - 1);
            float angle = Mathf.Lerp(-fovAngle / 2f, fovAngle / 2f, t);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
            coneRenderer.SetPosition(i + 1, origin + dir * fovRange);
        }

        coneRenderer.SetPosition(coneResolution + 1, origin);
    }

    void SetState(FOVState newState)
    {
        if (newState == CurrentState) return;
        CurrentState = newState;
        OnFOVStateChanged?.Invoke(CurrentState);
        UpdateConeColor();
        Debug.Log($"{gameObject.name} FOV: {CurrentState}");

        if (newState == FOVState.Suspicious && alarmRoutine == null)
            alarmRoutine = StartCoroutine(AlarmTick());
        else if (newState == FOVState.Unaware && alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }
    }

    private System.Collections.IEnumerator AlarmTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (DetectedPlayer != null)
            {
                InfiltratorAbility abilities = DetectedPlayer.GetComponent<InfiltratorAbility>();
                bool isHidden = abilities != null && abilities.isHidden;

                if (!isHidden)
                    AlarmSystem.Instance?.IncreaseAlarm();
                else
                {
                    alarmRoutine = null;
                    yield break;
                }
            }
            else
            {
                alarmRoutine = null;
                yield break;
            }
        }
    }

    void UpdateConeColor()
    {
        if (coneRenderer == null) return;

        Color c = CurrentState switch
        {
            FOVState.Suspicious => new Color(1f, 0.5f, 0f),
            FOVState.Alert      => Color.red,
            _                   => Color.green
        };

        coneRenderer.startColor = c;
        coneRenderer.endColor   = c;
    }

    public void TriggerAlert() => SetState(FOVState.Alert);

    public void ResetToUnaware()
    {
        DetectedPlayer = null;
        if (alarmRoutine != null) { StopCoroutine(alarmRoutine); alarmRoutine = null; }
        SetState(FOVState.Unaware);
    }

    void OnDrawGizmos()
    {
        Vector3 forward = Application.isPlaying ? GetForward()
            : Quaternion.Euler(0f, fovForwardOffset, 0f) * transform.forward;

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Vector3 left  = Quaternion.Euler(0, -fovAngle / 2f, 0) * forward * fovRange;
        Vector3 right = Quaternion.Euler(0,  fovAngle / 2f, 0) * forward * fovRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawLine(transform.position + left, transform.position + right);
    }
}