using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor alat koji od MASH "Sci Fi Gates" modela napravi gotov, funkcionalan
/// SciFiGate objekt: doda collidere (po stvarnim mesh granicama), postavi Wall
/// layer, doda i spoji SciFiGate skriptu (panelA = Door_Left, panelB = Door_Right)
/// i po zelji spremi kao prefab u Assets/Prefabs.
///
/// Koristenje:
/// 1) Povuci "Sci Fi Gates" prefab iz Projecta u scenu.
/// 2) Oznaci ga u Hierarchyju.
/// 3) Meni: Tools > Three Thieves > Setup Sci-Fi Gate (Selected).
/// </summary>
public static class SciFiGateSetup
{
    const string WallLayerName = "Wall";
    const string PrefabFolder = "Assets/Prefabs";

    [MenuItem("Tools/Three Thieves/Setup Sci-Fi Gate (Selected)")]
    static void SetupSelected()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            EditorUtility.DisplayDialog("Sci-Fi Gate",
                "Prvo oznaci 'Sci Fi Gates' objekt u sceni (Hierarchy).", "OK");
            return;
        }

        int wallLayer = LayerMask.NameToLayer(WallLayerName);
        if (wallLayer < 0)
        {
            EditorUtility.DisplayDialog("Sci-Fi Gate",
                $"Layer '{WallLayerName}' ne postoji. Dodaj ga u Project Settings > Tags and Layers.", "OK");
            return;
        }

        Transform left = FindChild(root.transform, "Door_Left") ?? FindChild(root.transform, "Left");
        Transform right = FindChild(root.transform, "Door_Right") ?? FindChild(root.transform, "Right");

        if (left == null && right == null)
        {
            EditorUtility.DisplayDialog("Sci-Fi Gate",
                "Nisu pronadjena krila (Door_Left / Door_Right) medju djecom oznacenog objekta.\n" +
                "Oznaci root 'Sci Fi Gates' objekt.", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(root, "Setup Sci-Fi Gate");

        float slideGuess = 1.5f;
        if (left != null) slideGuess = Mathf.Max(slideGuess, SetupPanel(left, wallLayer));
        if (right != null) slideGuess = Mathf.Max(slideGuess, SetupPanel(right, wallLayer));

        SciFiGate gate = root.GetComponent<SciFiGate>();
        if (gate == null) gate = Undo.AddComponent<SciFiGate>(root);

        gate.panelA = left != null ? left : right;
        gate.panelB = (left != null && right != null) ? right : null;
        // Split vrata se razmicu u stranu; panelB ide automatski suprotno (panelBMirrorsA).
        gate.directionA = SciFiGate.SlideDirection.Left;
        gate.distanceA = Mathf.Round(slideGuess * 100f) / 100f;
        gate.panelBMirrorsA = true;
        gate.requiredCharacter = CharacterType.None;

        EditorUtility.SetDirty(root);

        bool save = EditorUtility.DisplayDialog("Sci-Fi Gate",
            "Vrata su postavljena:\n" +
            $"- panelA = {(gate.panelA ? gate.panelA.name : "-")}\n" +
            $"- panelB = {(gate.panelB ? gate.panelB.name : "-")}\n" +
            $"- Wall layer + BoxCollideri dodani\n" +
            $"- distanceA ≈ {gate.distanceA} (prilagodi po potrebi)\n\n" +
            "Spremiti kao prefab u Assets/Prefabs?",
            "Spremi prefab", "Ostavi u sceni");

        if (save)
            SavePrefab(root);
    }

    /// <summary>Doda/namjesti BoxCollider na krilo i postavi Wall layer. Vraca sirinu krila (za slideDistance guess).</summary>
    static float SetupPanel(Transform panel, int wallLayer)
    {
        panel.gameObject.layer = wallLayer;

        BoxCollider box = panel.GetComponent<BoxCollider>();
        if (box == null) box = Undo.AddComponent<BoxCollider>(panel.gameObject);

        MeshFilter mf = panel.GetComponent<MeshFilter>();
        if (mf == null) mf = panel.GetComponentInChildren<MeshFilter>();

        float width = 1.5f;
        if (mf != null && mf.sharedMesh != null)
        {
            Bounds b = mf.sharedMesh.bounds; // lokalni prostor (bez scale-a) - tocno za BoxCollider
            box.center = b.center;
            box.size = b.size;
            // Sirina u svijetu = najveci horizontalni raspon * lossyScale.
            Vector3 s = panel.lossyScale;
            width = Mathf.Max(b.size.x * Mathf.Abs(s.x), b.size.z * Mathf.Abs(s.z));
        }
        return width;
    }

    static void SavePrefab(GameObject root)
    {
        if (!Directory.Exists(PrefabFolder))
            Directory.CreateDirectory(PrefabFolder);

        string path = AssetDatabase.GenerateUniqueAssetPath($"{PrefabFolder}/SciFiGate_Gates.prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.UserAction);

        if (prefab != null)
        {
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Sci-Fi Gate prefab spremljen: {path}");
        }
        else
        {
            Debug.LogError("Spremanje prefaba nije uspjelo.");
        }
    }

    /// <summary>Rekurzivno trazi dijete po imenu (case-insensitive contains).</summary>
    static Transform FindChild(Transform parent, string nameContains)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(nameContains.ToLower()))
                return child;

            Transform found = FindChild(child, nameContains);
            if (found != null) return found;
        }
        return null;
    }
}
