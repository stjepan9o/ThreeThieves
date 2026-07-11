using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Among Us - Wire Task Minigame (spajanje zica).
/// Hacker ga otvara da otkljuca nesto (gate, terminal...). Kad su sve zice
/// spojene, okine se onTaskComplete (npr. gate.ForceOpen).
///
/// SETUP:
/// 1. Canvas (Screen Space - Overlay) -> Panel "WireTaskPanel" (raycast target ukljucen,
///    da blokira klikove na svijet iza).
/// 2. Prazan GameObject "WireTaskManager" + ova skripta.
/// 3. 4 lijeva utikaca (Image + WirePlug) i 4 desna prikljucka (Image + WireSocket).
///    - plug[i] se spaja u socket[i] (isti index = ista boja).
///    - IZAZOV: posloži socket-e po VISINI drugacijim redom nego plug-ove, da se
///      zice moraju vuci dijagonalno (kao u Among Us). Boje se dodjeljuju po indexu.
/// 4. Poveži plugs[] i sockets[] u Inspectoru (isti broj elemenata).
/// 5. onTaskComplete -> npr. SciFiGate.ForceOpen.
/// </summary>
public class WireTaskManager : MonoBehaviour
{
    /// <summary>Dok je true, ostale skripte ignoriraju input (kretanje, prebacivanje likova).</summary>
    public static bool IsMinigameOpen { get; private set; }

    [Header("Wire Pairs (plug i socket istog indexa = ista boja)")]
    public WirePlug[] plugs;       // Lijevi utikaci (drag source)
    public WireSocket[] sockets;   // Desni prikljucci (drop targets)

    [Header("Boje zica")]
    public Color[] wireColors = new Color[]
    {
        new Color(0.196f, 0.302f, 0.612f), // plava
        new Color(0.898f, 0.137f, 0.125f), // crvena
        new Color(1.000f, 0.922f, 0.075f), // zuta
        new Color(0.651f, 0.322f, 0.604f)  // ljubicasta
    };

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip taskCompleteSound;

    [Header("UI References")]
    public GameObject taskPanel;   // Cijeli panel minigame (show/hide)
    public Text statusText;        // Opcionalno - prikaz statusa

    [Header("Na kraju (sve zice spojene)")]
    public bool closeOnComplete = true;
    public UnityEvent onTaskComplete;   // npr. gate.ForceOpen

    private bool[] connected;
    private bool taskCompleted = false;
    private System.Action onCompleteOnce;   // runtime callback (opcionalno)

    void Start()
    {
        connected = new bool[plugs.Length];

        for (int i = 0; i < plugs.Length; i++)
        {
            if (i < wireColors.Length)
            {
                plugs[i].wireColor = wireColors[i];
                sockets[i].wireColor = wireColors[i];
            }
            plugs[i].taskManager = this;
            plugs[i].plugIndex = i;
            sockets[i].socketIndex = i;
            plugs[i].Initialize();
            sockets[i].Initialize();
        }

        // Panel je skriven dok ga netko ne otvori.
        if (taskPanel != null && taskPanel.activeSelf)
            taskPanel.SetActive(false);
    }

    /// <summary>Otvori minigame. Opcionalni callback se okine kad se zavrsi (uz onTaskComplete).</summary>
    public void ShowTask(System.Action onComplete = null)
    {
        onCompleteOnce = onComplete;
        taskCompleted = false;

        if (connected != null)
            for (int i = 0; i < connected.Length; i++)
            {
                connected[i] = false;
                if (i < plugs.Length) plugs[i].ResetPosition();
                if (i < sockets.Length) sockets[i].SetConnected(false);
            }

        if (statusText) statusText.text = "";
        if (taskPanel) taskPanel.SetActive(true);
        IsMinigameOpen = true;
    }

    /// <summary>Zatvori minigame bez zavrsavanja (npr. gumb X).</summary>
    public void CloseTask()
    {
        if (taskPanel) taskPanel.SetActive(false);
        IsMinigameOpen = false;
    }

    /// <summary>Poziva WirePlug kad korisnik otpusti zicu.</summary>
    public void OnPlugReleased(int plugIndex, WireSocket targetSocket)
    {
        if (targetSocket != null && targetSocket.socketIndex == plugIndex)
        {
            connected[plugIndex] = true;
            plugs[plugIndex].SnapToSocket(targetSocket);
            targetSocket.SetConnected(true);
            CheckAllConnected();
        }
        else
        {
            connected[plugIndex] = false;
            plugs[plugIndex].ResetPosition();
            if (targetSocket != null)
                targetSocket.SetConnected(false);
        }
    }

    public void DisconnectPlug(int plugIndex)
    {
        connected[plugIndex] = false;
        sockets[plugIndex].SetConnected(false);
    }

    void CheckAllConnected()
    {
        if (taskCompleted) return;

        foreach (bool c in connected)
            if (!c) return;

        taskCompleted = true;
        StartCoroutine(CompleteTask());
    }

    IEnumerator CompleteTask()
    {
        if (statusText) statusText.text = "TASK COMPLETE!";
        if (audioSource && taskCompleteSound)
            audioSource.PlayOneShot(taskCompleteSound);

        yield return new WaitForSeconds(1f);

        IsMinigameOpen = false;

        // Okini akcije otkljucavanja.
        onTaskComplete?.Invoke();
        onCompleteOnce?.Invoke();
        onCompleteOnce = null;

        if (statusText) statusText.text = "";
        if (closeOnComplete && taskPanel)
            taskPanel.SetActive(false);
    }
}
