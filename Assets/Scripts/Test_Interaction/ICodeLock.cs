/// <summary>
/// Zajednicko sucelje za sve sto se otkljucava kodom preko KeypadUI-a
/// (CodeDoor, CodeGate...). KeypadUI radi s ovim suceljem umjesto s
/// konkretnim tipom vrata, pa isti keypad otvara i obicna i sci-fi vrata.
/// </summary>
public interface ICodeLock
{
    /// <summary>Vraca true ako je kod tocan (i otvara vrata), false inace.</summary>
    bool TryCode(string enteredCode);
}
