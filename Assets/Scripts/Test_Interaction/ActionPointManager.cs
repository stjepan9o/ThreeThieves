using UnityEngine;

// Postavi ovu skriptu na prazan GameObject koji ces nazvati "GameManager"
// Upravlja Action Pointima tima
public class ActionPointManager : MonoBehaviour
{
    [Header("Action Points")]
    public int maxAP = 6;           // Maksimalni AP po potezu
    public int CurrentAP { get; private set; }

    void Start()
    {
        CurrentAP = maxAP;
        Debug.Log($"Potez poceo! AP: {CurrentAP}/{maxAP}");
    }

    public bool HasEnoughAP(int cost)
    {
        return CurrentAP >= cost;
    }

    public void SpendAP(int amount)
    {
        CurrentAP -= amount;
        CurrentAP = Mathf.Max(0, CurrentAP); // Ne moze ici ispod 0
        Debug.Log($"Potroseno {amount} AP. Preostalo: {CurrentAP}/{maxAP}");
    }

    // Resetiraj AP na pocetku novog poteza
    public void ResetAP()
    {
        CurrentAP = maxAP;
        Debug.Log($"Novi potez! AP resetiran na {maxAP}");
    }

    // Tipka E za reset AP (za testiranje)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ResetAP();
        }
    }
}
