using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class NPCInteraction : MonoBehaviour
{
    public string playerTag = "Player";
    public AudioClip interactionSound;
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public string[] dialogueLines;
    public GameObject questUI;
    public TextMeshProUGUI questText;
    public string questDescription;

    public ScreenSpaceDialogue screenSpaceDialogue;

    private AudioSource audioSource;
    private int currentDialogueIndex = 0;
    private bool isPlayerInRange = false;
    public bool questCompleted = false;

    public enum DialogueMode { WorldSpace, ScreenSpace }
    public DialogueMode dialogueMode = DialogueMode.WorldSpace;
    public QuestKillEnemies killEnemiesQuest;

    private PlayerMovement playerMovement;
    private float originalMoveSpeed;
    private float originalJumpForce;
    private QuestPanelAnimator questPanelAnimator;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
        if (screenSpaceDialogue != null)
        {
            screenSpaceDialogue.HideDialogue();
        }
        if (questUI != null)
        {
            questPanelAnimator = questUI.GetComponent<QuestPanelAnimator>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            PlayInteractionSound();

            playerMovement = other.GetComponent<PlayerMovement>();

            if (!questCompleted)
            {
                StartDialogue();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            EndDialogue();
        }
    }

    void PlayInteractionSound()
    {
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }

    void StartDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            if (playerMovement != null)
            {
                originalMoveSpeed = playerMovement.moveSpeed;
                originalJumpForce = playerMovement.jumpForce;

                playerMovement.moveSpeed = 0f;
                playerMovement.jumpForce = 0f;
            }

            if (dialogueMode == DialogueMode.WorldSpace)
            {
                if (dialogueUI != null)
                {
                    dialogueUI.SetActive(true);
                    dialogueText.text = dialogueLines[currentDialogueIndex];
                }
            }
            else if (dialogueMode == DialogueMode.ScreenSpace)
            {
                if (screenSpaceDialogue != null)
                {
                    screenSpaceDialogue.ShowDialogue(dialogueLines[currentDialogueIndex]);
                }
            }
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!questCompleted)
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < dialogueLines.Length)
                {
                    if (dialogueMode == DialogueMode.WorldSpace)
                    {
                        if (dialogueUI != null)
                        {
                            dialogueText.text = dialogueLines[currentDialogueIndex];
                        }
                    }
                    else if (dialogueMode == DialogueMode.ScreenSpace)
                    {
                        if (screenSpaceDialogue != null)
                        {
                            screenSpaceDialogue.ShowDialogue(dialogueLines[currentDialogueIndex]);
                        }
                    }
                }
                else
                {
                    EndDialogue();
                    AddQuestToUI();
                }
            }
        }
    }

    void EndDialogue()
    {
        if (dialogueMode == DialogueMode.WorldSpace)
        {
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
        }
        else if (dialogueMode == DialogueMode.ScreenSpace)
        {
            if (screenSpaceDialogue != null)
            {
                screenSpaceDialogue.HideDialogue();
            }
        }

        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalMoveSpeed;
            playerMovement.jumpForce = originalJumpForce;
        }

        currentDialogueIndex = 0;
    }

    void AddQuestToUI()
    {
        if (questUI != null && questText != null)
        {
            questUI.SetActive(true);
            if (questPanelAnimator != null)
                questPanelAnimator.PlayOpenAnimation(questDescription);
            else
                questText.text = questDescription;

            // Start the kill enemies quest if assigned to this NPC
            if (killEnemiesQuest != null)
                killEnemiesQuest.StartQuest();

            // If you have other quest types, call their StartQuest() here
            // if (fetchItemQuest != null)
            //     fetchItemQuest.StartQuest();
        }
    }

    public void CompleteQuest()
    {
        if (questUI != null && questText != null)
        {
            questText.text = "Quest Completed!";
            questCompleted = true;
            StartCoroutine(FadeOutQuestText());
        }
    }

    private IEnumerator FadeOutQuestText()
    {
        yield return new WaitForSeconds(5f);

        Color originalColor = questText.color;
        float fadeDuration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            questText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        questText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Start the shrink animation after fading out the text
        if (questPanelAnimator != null)
            yield return questPanelAnimator.PlayShrinkAnimation();

        //questUI.SetActive(false);
    }
}
