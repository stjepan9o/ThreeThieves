using UnityEngine;
using TMPro;

public class HackerCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public TextMeshProUGUI cameraDisplayText;

    private Camera[] securityCameras;
    private int currentCameraIndex = 0;
    private Camera currentCamera;
    private bool hackerActive = false;

    void Start()
    {
        // Sakrij tekst na pocetku
        if (cameraDisplayText != null)
            cameraDisplayText.gameObject.SetActive(false);

        // Pronadji sve security kamere
        GameObject[] cameraObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        System.Collections.Generic.List<Camera> cameras = new System.Collections.Generic.List<Camera>();
        foreach (GameObject obj in cameraObjects)
        {
            if (obj.name.Contains("security_camera"))
            {
                Camera cam = obj.GetComponent<Camera>();
                if (cam == null)
                    cam = obj.GetComponentInChildren<Camera>();
                if (cam != null)
                    cameras.Add(cam);
            }
        }
        securityCameras = cameras.ToArray();

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

            SetActiveCamera(currentCameraIndex);
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
        }

        // Kontrole samo kad je Hacker aktivan
        if (!hackerActive || securityCameras.Length == 0) return;

        // Dok je keypad otvoren - ignoriraj prebacivanje kamera
        if (KeypadUI.IsKeypadOpen) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetActiveCamera((currentCameraIndex + 1) % securityCameras.Length);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetActiveCamera((currentCameraIndex - 1 + securityCameras.Length) % securityCameras.Length);

        for (int i = 0; i < securityCameras.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SetActiveCamera(i);
        }
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
            cameraDisplayText.text = $"CCTV {currentCameraIndex + 1}/{securityCameras.Length}\n{currentCamera.gameObject.name}";

        Debug.Log($"Prebacena na kameru: {currentCamera.gameObject.name}");
    }

    public int GetCurrentCameraIndex() => currentCameraIndex;
    public Camera GetCurrentCamera() => currentCamera;
}