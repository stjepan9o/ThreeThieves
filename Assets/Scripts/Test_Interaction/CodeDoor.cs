using UnityEngine;

/// <summary>
/// Vrata koja se otkljucavaju upisivanjem koda.
/// Nasljedjuje Door - koristi istu animaciju otvaranja i grid refresh,
/// ali umjesto direktnog otvaranja prvo otvara Keypad UI.
///
/// Setup u Unity:
/// 1) Na vrata prije vaulta stavi OVU skriptu umjesto Door.
/// 2) U Inspectoru upisi correctCode (npr. "4821").
/// 3) Povuci KeypadUI objekt iz Hierarchyja u keypadUI polje.
/// </summary>
public class CodeDoor : Door, ICodeLock
{
    [Header("Code Settings")]
    public string correctCode = "1234";
    public KeypadUI keypadUI;

    protected override void OnInteract()
    {
        if (isOpen || isAnimating) return;

        if (keypadUI == null)
        {
            Debug.LogWarning($"{gameObject.name}: KeypadUI nije povucen u Inspector!");
            return;
        }

        // Umjesto otvaranja vrata - otvori keypad
        keypadUI.Open(this);
    }

    /// <summary>
    /// Poziva KeypadUI kad igrac klikne Potvrdi.
    /// Vraca true ako je kod tocan (vrata se otvaraju), false ako nije.
    /// </summary>
    public bool TryCode(string enteredCode)
    {
        if (enteredCode == correctCode)
        {
            Debug.Log($"{gameObject.name}: Tocan kod! Vrata se otvaraju.");
            ForceOpen();
            return true;
        }

        Debug.Log($"{gameObject.name}: Krivi kod ({enteredCode})!");
        return false;
    }
}
