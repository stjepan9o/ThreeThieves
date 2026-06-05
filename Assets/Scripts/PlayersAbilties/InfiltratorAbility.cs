using UnityEngine;

public class InfiltratorAbility : MonoBehaviour
{
 public bool isHidden = false;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            isHidden = !isHidden;
            Debug.Log("Hide ability: " + isHidden);
        }
    }
}
