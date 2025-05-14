using System;
using UnityEngine;

public class PlayerDialogue : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text dialogueText;

    [SerializeField] private float dialogueAlphaDecay = 0.5f;
    private float _dialogueTimer;

    public void OnEnable()
    {
        dialogueText.text = "";
        dialogueText.alpha = 0;
        _dialogueTimer = 0;
    }

    private void Update()
    {
        _dialogueTimer -= (_dialogueTimer > 0) ? Time.deltaTime : 0;

        if (_dialogueTimer <= 0 && dialogueText.alpha > 0)
        {
            dialogueText.alpha = Mathf.Clamp(dialogueText.alpha - dialogueAlphaDecay*Time.deltaTime, 0, 1);

            if (dialogueText.alpha <= 0)
                dialogueText.text = "";
        }
    }
    
    public void SetDialogue(string dialogue, float duration = 2.5f) 
    {
        dialogueText.text = dialogue;
        dialogueText.alpha = 1;
        _dialogueTimer = duration;
    }
}
