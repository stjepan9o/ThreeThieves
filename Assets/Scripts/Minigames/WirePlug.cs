using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Lijevi utikac (drag source). Stavi na svaki lijevi kruzic.
/// Objekt treba imati Image (Raycast Target ukljucen).
/// </summary>
public class WirePlug : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public WireTaskManager taskManager;
    [HideInInspector] public int plugIndex;
    [HideInInspector] public Color wireColor = Color.white;

    [Header("Vizualni elementi")]
    public Image plugImage;              // Slika utikaca
    public RectTransform lineUI;         // UI Image za zicu (pivot lijevo-sredina 0,0.5) - opcionalno

    private Vector2 startPosition;
    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;
    private bool isConnected = false;
    private WireSocket currentSocket = null;

    public void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition;

        if (plugImage == null)
            plugImage = GetComponent<Image>();
        if (plugImage != null)
            plugImage.color = wireColor;

        UpdateWireLine();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Dovedi zicu na vrh da se crta iznad ostalih.
        transform.SetAsLastSibling();

        if (isConnected)
        {
            taskManager.DisconnectPlug(plugIndex);
            isConnected = false;
            currentSocket = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentRect == null) return;

        // VAZNO: racunamo lokalnu tocku u prostoru RODITELJA utikaca (ne canvasa),
        // inace utikac "skace" ako nije direktno dijete canvasa.
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position,
                canvas != null ? canvas.worldCamera : null,
                out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
            UpdateWireLine();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        WireSocket hitSocket = null;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            var socket = result.gameObject.GetComponent<WireSocket>();
            if (socket != null)
            {
                hitSocket = socket;
                break;
            }
        }

        taskManager.OnPlugReleased(plugIndex, hitSocket);
    }

    public void SnapToSocket(WireSocket socket)
    {
        isConnected = true;
        currentSocket = socket;
        StopAllCoroutines();
        StartCoroutine(AnimateSnap(socket.GetComponent<RectTransform>().anchoredPosition));
    }

    IEnumerator AnimateSnap(Vector2 targetPos)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            UpdateWireLine();
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;
        UpdateWireLine();
    }

    public void ResetPosition()
    {
        isConnected = false;
        currentSocket = null;
        StopAllCoroutines();
        if (gameObject.activeInHierarchy)
            StartCoroutine(AnimateReset());
        else
        {
            rectTransform.anchoredPosition = startPosition;
            UpdateWireLine();
        }
    }

    IEnumerator AnimateReset()
    {
        Vector2 from = rectTransform.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.3f;
            rectTransform.anchoredPosition = Vector2.Lerp(from, startPosition, Mathf.SmoothStep(0, 1, t));
            UpdateWireLine();
            yield return null;
        }
        rectTransform.anchoredPosition = startPosition;
        UpdateWireLine();
    }

    /// <summary>
    /// Crta UI "zicu" od pocetne (lijeve) tocke do trenutne pozicije utikaca.
    /// Zahtijeva lineUI Image s pivotom (0, 0.5). Ako nije postavljen, preskace se.
    /// </summary>
    void UpdateWireLine()
    {
        if (lineUI == null || rectTransform == null) return;

        Vector2 from = startPosition;
        Vector2 to = rectTransform.anchoredPosition;
        Vector2 dir = to - from;

        lineUI.anchoredPosition = from;
        lineUI.sizeDelta = new Vector2(dir.magnitude, lineUI.sizeDelta.y);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineUI.localRotation = Quaternion.Euler(0, 0, angle);

        Image img = lineUI.GetComponent<Image>();
        if (img != null) img.color = wireColor;
    }
}
