using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Editor alat koji jednim klikom izgradi cijeli Wire minigame UI
/// (Canvas + EventSystem po potrebi, panel, 4 zice + 4 socketa, boje,
/// sve reference povezane na WireTaskManager).
///
/// Meni: Tools > Three Thieves > Build Wire Minigame.
/// </summary>
public static class WireMinigameBuilder
{
    static readonly Color[] Colors =
    {
        new Color(0.196f, 0.302f, 0.612f), // plava
        new Color(0.898f, 0.137f, 0.125f), // crvena
        new Color(1.000f, 0.922f, 0.075f), // zuta
        new Color(0.651f, 0.322f, 0.604f)  // ljubicasta
    };

    static readonly float[] Ys = { 180f, 60f, -60f, -180f };
    static readonly int[] SocketOrder = { 2, 0, 3, 1 }; // socketi na drugim visinama -> dijagonalne zice

    [MenuItem("Tools/Three Thieves/Build Wire Minigame")]
    static void Build()
    {
        Canvas canvas = GetOrCreateCanvas();
        EnsureEventSystem();

        Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Manager (uvijek aktivan) + panel (toggla se).
        GameObject managerGO = NewUI("WireTask", canvas.transform);
        var manager = managerGO.AddComponent<WireTaskManager>();

        GameObject panelGO = NewUI("WireTaskPanel", managerGO.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.06f, 0.09f, 0.92f);
        panelImg.raycastTarget = true;

        int n = 4;
        var plugs = new WirePlug[n];
        var sockets = new WireSocket[n];

        for (int i = 0; i < n; i++)
        {
            // Zica (iza utikaca, isti roditelj = panel).
            var lineGO = NewUI($"Wire_{i}", panelGO.transform);
            var lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.anchorMin = lineRT.anchorMax = new Vector2(0.5f, 0.5f);
            lineRT.pivot = new Vector2(0f, 0.5f);
            lineRT.anchoredPosition = new Vector2(-300f, Ys[i]);
            lineRT.sizeDelta = new Vector2(0f, 12f);
            var lineImg = lineGO.AddComponent<Image>();
            lineImg.color = Colors[i];
            lineImg.raycastTarget = false;

            // Plug (ispred zice).
            var plugImg = MakeCircle($"Plug_{i}", panelGO.transform, new Vector2(-300f, Ys[i]), 64f, Colors[i], circle, true);
            var plug = plugImg.gameObject.AddComponent<WirePlug>();
            plug.plugImage = plugImg;
            plug.lineUI = lineRT;
            plugs[i] = plug;

            // Socket (druga visina).
            var sockImg = MakeCircle($"Socket_{i}", panelGO.transform, new Vector2(300f, Ys[SocketOrder[i]]), 64f, Colors[i], circle, true);
            sockets[i] = sockImg.gameObject.AddComponent<WireSocket>();
        }

        // Gumb X (zatvori bez zavrsavanja).
        var closeImg = MakeCircle("CloseButton", panelGO.transform, new Vector2(430f, 240f), 48f, new Color(0.8f, 0.2f, 0.2f), circle, true);
        var closeBtn = closeImg.gameObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(closeBtn.onClick, manager.CloseTask);

        // Poveži reference.
        manager.plugs = plugs;
        manager.sockets = sockets;
        manager.wireColors = Colors;
        manager.taskPanel = panelGO;

        Undo.RegisterCreatedObjectUndo(managerGO, "Build Wire Minigame");
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(managerGO.scene);
        Selection.activeGameObject = managerGO;

        EditorUtility.DisplayDialog("Wire Minigame",
            "Minigame izgradjen: Canvas > WireTask > WireTaskPanel.\n\n" +
            "Preostalo:\n" +
            "- Na WireTaskManager postavi onTaskComplete (npr. otkljucaj gate) ako treba.\n" +
            "- Poveži HackTerminal.wireTask na ovaj WireTask objekt.\n" +
            "- Po zelji preraspored socket-e po visini.", "OK");
    }

    static Image MakeCircle(string name, Transform parent, Vector2 pos, float size, Color color, Sprite sprite, bool raycast)
    {
        var go = NewUI(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        GameObjectUtility.SetParentAndAlign(go, parent != null ? parent.gameObject : null);
        return go;
    }

    static Canvas GetOrCreateCanvas()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // Drag radi samo ako Canvas ima GraphicRaycaster.
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
        return canvas;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
    }
}
