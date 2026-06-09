using UnityEngine;
using System.Collections;

public class Door : InteractableObject
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool openInward = false;

    [Header("Character Restriction")]
    public CharacterType requiredCharacter = CharacterType.None;

    private bool isOpen = false;
    private bool isAnimating = false;
    public GameObject doorMesh;

    void Start()
    {
        interactionPrompt = "Otvori vrata";
        apCost = 2;
    }

    protected override void OnInteract()
    {
        if (isOpen || isAnimating) return;

        // Provjeri je li pravi lik aktivan
        if (requiredCharacter != CharacterType.None)
        {
            if (CharacterSwitcher.Instance == null)
            {
                Debug.LogWarning("CharacterSwitcher nije pronadjen!");
                return;
            }

            CharacterType activeType = CharacterSwitcher.Instance.ActiveCharacterType;

            if (activeType != requiredCharacter)
            {
                Debug.Log($"Ova vrata moze otvoriti samo {requiredCharacter}! Trenutno aktivan: {activeType}");
                return;
            }
        }

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
        gameObject.layer = LayerMask.NameToLayer("Default");
        isOpen = true;
        isAnimating = false;
        Debug.Log("Vrata su otvorena!");
    }
}