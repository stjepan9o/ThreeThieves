using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Desni prikljucak (drop target). Stavi na svaki desni kruzic.
/// Objekt treba imati Image (Raycast Target ukljucen da ga drag prepozna).
/// </summary>
public class WireSocket : MonoBehaviour
{
    [HideInInspector] public int socketIndex;
    [HideInInspector] public Color wireColor = Color.white;

    private Image socketImage;
    private bool isConnected = false;

    public void Initialize()
    {
        socketImage = GetComponent<Image>();
        if (socketImage != null)
            socketImage.color = wireColor;
    }

    public void SetConnected(bool connected)
    {
        isConnected = connected;

        if (socketImage != null)
        {
            // Posvijetli kad je spojeno.
            socketImage.color = connected
                ? Color.Lerp(wireColor, Color.white, 0.5f)
                : wireColor;
        }
    }

    public bool IsConnected => isConnected;
}
