using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Content")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Settings")]
    public float delayBeforeStart = 3f;   // 几秒后开始显示

    private int currentLineIndex = 0;
    private bool dialogueActive = false;
    private bool dialogueFinished = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
        StartCoroutine(StartDialogueWithDelay());
    }

    IEnumerator StartDialogueWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        dialogueActive = true;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentLineIndex];
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        if (dialogueFinished) return;

        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            dialogueFinished = true;
            dialogueActive = false;

            // 这里不会关闭对话框
            // 只是停在最后一句
        }
    }
}
