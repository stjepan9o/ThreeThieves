using UnityEngine;

/// <summary>
/// Sci-Fi Gate koji se otkljucava upisivanjem koda.
/// Nasljedjuje SciFiGate (isto klizanje krila, layeri, grid refresh), ali
/// umjesto direktnog otvaranja prvo otvori isti KeypadUI kao CodeDoor.
///
/// Setup u Unity:
/// 1) Na gate objekt (umjesto SciFiGate) stavi OVU skriptu i namjesti
///    panele/smjer kao i inace (CodeGate ima sva polja SciFiGate-a).
/// 2) U Inspectoru upisi correctCode (npr. "4821").
/// 3) Povuci KeypadUI objekt iz Hierarchyja u keypadUI polje.
/// </summary>
public class CodeGate : SciFiGate, ICodeLock
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

        // Umjesto otvaranja vrata - otvori keypad.
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
