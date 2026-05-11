using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameState.Instance.PickupKeycard();
        Destroy(gameObject);
    }
}