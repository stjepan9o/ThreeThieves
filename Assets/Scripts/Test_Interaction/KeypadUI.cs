using UnityEngine;
using TMPro;

/// <summary>
/// UI panel za upisivanje koda na CodeDoor vratima.
///
/// Setup u Unity:
/// 1) Na Canvasu: desni klik -> UI -> Panel, nazovi ga "KeypadPanel".
/// 2) U panel dodaj:
///    - TMP Input Field (za upis koda)
///    - Button "Potvrdi"
///    - Button "X" (zatvori)
///    - TMP Text (za poruku "KRIVI KOD!")
/// 3) Ovu skriptu stavi na Canvas (ili neki objekt koji je UVIJEK aktivan,
///    NE na sam panel - jer ako je panel iskljucen, skripta ne radi).
/// 4) Povuci reference: panel, codeInput, feedbackText.
/// 5) Na gumb Potvrdi: OnClick -> KeypadUI.OnConfirm
/// 6) Na gumb X: OnClick -> KeypadUI.OnClose
/// 7) Deaktiviraj KeypadPanel (checkbox) da nije vidljiv na startu.
/// </summary>
public class KeypadUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;           // cijeli KeypadPanel
    public TMP_InputField codeInput;   // polje za upis koda
    public TMP_Text feedbackText;      // poruka "KRIVI KOD!"

    /// <summary>
    /// Globalna zastavica - dok je keypad otvoren, ostale skripte (CharacterSwitcher,
    /// HackerCameraController) ignoriraju svoj input da se ne mijenjaju likovi/kamere
    /// dok igrac tipka kod.
    /// </summary>
    public static bool IsKeypadOpen = false;

    private ICodeLock currentLock;

    /// <summary>Poziva CodeDoor / CodeGate kad igrac klikne na vrata.</summary>
    public void Open(ICodeLock codeLock)
    {
        currentLock = codeLock;
        codeInput.text = "";
        feedbackText.text = "";
        panel.SetActive(true);
        IsKeypadOpen = true;

        // Automatski fokusiraj input polje da igrac moze odmah tipkati
        codeInput.Select();
        codeInput.ActivateInputField();
    }

    /// <summary>Poziva se na klik gumba "Potvrdi".</summary>
    public void OnConfirm()
    {
        if (currentLock == null) return;

        if (currentLock.TryCode(codeInput.text))
        {
            // Tocan kod - zatvori panel
            panel.SetActive(false);
            IsKeypadOpen = false;
            currentLock = null;
        }
        else
        {
            // Krivi kod - poruka i ocisti polje
            feedbackText.text = "KRIVI KOD!";
            codeInput.text = "";
            codeInput.Select();
            codeInput.ActivateInputField();
        }
    }

    /// <summary>Poziva se na klik gumba "X" (zatvori bez otvaranja vrata).</summary>
    public void OnClose()
    {
        panel.SetActive(false);
        IsKeypadOpen = false;
        currentLock = null;
    }
}