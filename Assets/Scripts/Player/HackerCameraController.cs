using UnityEngine;
using TMPro;

public class HackerCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public TextMeshProUGUI cameraDisplayText;

    [Header("Hacked Feedback")]
    public GameObject hackedOverlay;

    [Header("Audio")]
    public AudioSource staticAudio;

    [Header("Controls")]
    public KeyCode cycleKey = KeyCode.Tab;

    private Camera[] securityCameras;
    private string[] cameraNames;
    private int[] cameraNumbers;
    private int currentCameraIndex = 0;
    private Camera currentCamera;
    private bool hackerActive = false;

    void Start()
    {
        // Sakrij tekst na pocetku
        if (cameraDisplayText != null)
            cameraDisplayText.gameObject.SetActive(false);

        if (hackedOverlay != null)
            hackedOverlay.SetActive(false);

        // Pronadji sve security kamere
        GameObject[] cameraObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var entries = new System.Collections.Generic.List<(string name, Camera cam)>();
        foreach (GameObject obj in cameraObjects)
        {
            if (!obj.name.StartsWith("SurveillanceCamera")) continue;
            if (obj.name.Contains("Zone") || obj.name.Contains("Mount")) continue;

            Camera cam = obj.GetComponentInChildren<Camera>(true);
            if (cam == null) continue;

            bool alreadyAdded = false;
            foreach (var e in entries)
                if (e.cam == cam) { alreadyAdded = true; break; }

            if (!alreadyAdded)
                entries.Add((obj.name, cam));
        }

        entries.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        securityCameras = new Camera[entries.Count];
        cameraNames = new string[entries.Count];
        cameraNumbers = new int[entries.Count];
        for (int i = 0; i < entries.Count; i++)
        {
            securityCameras[i] = entries[i].cam;
            cameraNames[i] = entries[i].name;
            cameraNumbers[i] = ParseCameraNumber(entries[i].name);
        }

        if (securityCameras.Length == 0)
        {
            Debug.LogError("Nema pronadjenih security kamera!");
            return;
        }

        Debug.Log($"Pronadjeno {securityCameras.Length} kamera!");

        // Iskljuci sve kamere na pocetku
        foreach (Camera cam in securityCameras)
            cam.enabled = false;
    }

    void Update()
    {
        // Provjeri je li Hacker aktivan
        bool isHackerNow = CharacterSwitcher.Instance != null &&
                           CharacterSwitcher.Instance.ActiveCharacterType == CharacterType.Hacker;

        // Kad se Hacker tek aktivira
        if (isHackerNow && !hackerActive)
        {
            hackerActive = true;
            if (cameraDisplayText != null)
                cameraDisplayText.gameObject.SetActive(true);

            if (mainCamera != null)
                mainCamera.enabled = false;

            if (staticAudio != null)
                staticAudio.Play();

            SetActiveCamera(0);
        }
        // Kad se Hacker deaktivira
        else if (!isHackerNow && hackerActive)
        {
            hackerActive = false;
            if (cameraDisplayText != null)
                cameraDisplayText.gameObject.SetActive(false);

            if (currentCamera != null)
                currentCamera.enabled = false;

            if (mainCamera != null)
                mainCamera.enabled = true;

            if (staticAudio != null)
                staticAudio.Stop();
        }

        UpdateHackedOverlay();

        // Kontrole samo kad je Hacker aktivan
        if (!hackerActive || securityCameras.Length == 0) return;

        // Dok je keypad otvoren - ignoriraj prebacivanje kamera
        if (KeypadUI.IsKeypadOpen) return;
        if (WireTaskManager.IsMinigameOpen) return;

        if (HackMinigame.Instance != null && HackMinigame.Instance.IsRunning) return;

        if (Input.GetKeyDown(cycleKey))
            SetActiveCamera((currentCameraIndex + 1) % securityCameras.Length);
    }


    private int ParseCameraNumber(string name)
    {
        var m = System.Text.RegularExpressions.Regex.Match(name, @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : -1;
    }

    public int GetCurrentCameraNumber()
    {
        if (cameraNumbers == null || cameraNumbers.Length == 0) return -1;
        return cameraNumbers[currentCameraIndex];
    }


    private void UpdateHackedOverlay()
    {
        if (hackedOverlay == null) return;

        bool showOverlay = hackerActive &&
                           CameraHackSystem.Instance != null &&
                           CameraHackSystem.Instance.IsHacked(GetCurrentCameraNumber());

        if (hackedOverlay.activeSelf != showOverlay)
            hackedOverlay.SetActive(showOverlay);
    }

    void SetActiveCamera(int index)
    {
        if (index < 0 || index >= securityCameras.Length) return;

        if (currentCamera != null)
            currentCamera.enabled = false;

        currentCameraIndex = index;
        currentCamera = securityCameras[currentCameraIndex];
        currentCamera.enabled = true;

        if (cameraDisplayText != null)
            cameraDisplayText.text = $"CCTV {currentCameraIndex + 1}/{securityCameras.Length}\n{cameraNames[currentCameraIndex]}";

        Debug.Log($"Aktivna kamera: {cameraNames[currentCameraIndex]}");
    }

    public int GetCurrentCameraIndex() => currentCameraIndex;
    public Camera GetCurrentCamera() => currentCamera;
}
