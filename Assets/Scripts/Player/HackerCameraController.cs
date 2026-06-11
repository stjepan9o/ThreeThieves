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
        
        if (cameraDisplayText != null)
            cameraDisplayText.gameObject.SetActive(false);

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
            Debug.LogError("Nema pronađenih security kamera!");
            return;
        }

        Debug.Log($"Pronađeno {securityCameras.Length} kamera!");

        foreach (Camera cam in securityCameras)
            cam.enabled = false;
    }

    void Update()
    {
        bool isHackerNow = CharacterSwitcher.Instance != null &&
                   CharacterSwitcher.Instance.ActiveCharacterType == CharacterType.Hacker;

        if (isHackerNow && !hackerActive)
        {
            hackerActive = true;
            if (cameraDisplayText != null)
                cameraDisplayText.gameObject.SetActive(true);

            if (mainCamera != null)
                mainCamera.enabled = false;

            SetActiveCamera(currentCameraIndex);
        }
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

        if (!hackerActive || securityCameras.Length == 0) return;

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

        Debug.Log($"Prebačena na kameru: {currentCamera.gameObject.name}");
    }

    public int GetCurrentCameraIndex() => currentCameraIndex;
    public Camera GetCurrentCamera() => currentCamera;
}