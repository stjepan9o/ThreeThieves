using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VaultInteraction : MonoBehaviour
{
    [Header("Vault Settings")]
    public Material completedMaterial;
    
    [Header("UI")]
    public GameObject missionCompleteUI;

    private void OnMouseDown()
    {
        if (GameState.Instance.hasKeycard)
        {
            GameState.Instance.CompleteMission();
            
            
            GetComponent<Renderer>().material = completedMaterial;
            
            
            if (missionCompleteUI != null)
                missionCompleteUI.SetActive(true);
        }
        else
        {
            Debug.Log("Potrebna keyecard prvo");
        }
    }
}