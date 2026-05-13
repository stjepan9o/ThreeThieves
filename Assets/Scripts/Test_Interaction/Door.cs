using UnityEngine;
using System.Collections;

public class Door : InteractableObject
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool openInward = false;

    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        interactionPrompt = "Otvori vrata";
        apCost = 2;
    }

    protected override void OnInteract()
    {
        if (isOpen || isAnimating) return;
        Debug.Log($"{gameObject.name}: Vrata se otvaraju!");
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        isAnimating = true;

        float targetAngle = openInward ? -openAngle : openAngle;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, targetAngle, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, Mathf.Clamp01(t));
            yield return null;
        }

        transform.rotation = endRotation;
        isOpen = true;
        isAnimating = false;
        Debug.Log("Vrata su otvorena!");
    }
}