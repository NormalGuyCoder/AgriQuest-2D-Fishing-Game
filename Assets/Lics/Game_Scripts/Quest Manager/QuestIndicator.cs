using UnityEngine;
using System.Collections.Generic;

public class QuestIndicator : MonoBehaviour
{
    [Header("Bubble Settings")]
    public GameObject questBubblePrefab;
    public Vector3 bubbleOffset = new Vector3(0, 2f, 0);

    [Header("Bubble Types")]
    public Sprite availableQuestSprite;
    public Sprite inProgressQuestSprite;

    private GameObject currentBubble;
    private SpriteRenderer bubbleSpriteRenderer;
    private string npcName;

    void Start()
    {
        var dialogInteractor = GetComponent<DialogInteractor>();
        if (dialogInteractor != null)
        {
            npcName = dialogInteractor.npcName;
        }
        else
        {
            npcName = gameObject.name;
        }

        if (questBubblePrefab != null)
        {
            CreateBubble();
        }
        else
        {
            CreateDefaultBubble();
        }

        if (currentBubble != null)
            currentBubble.SetActive(false);

        UpdateBubbleState();

        if (QuestManager.Instance != null)
        {
            QuestManager.OnQuestStarted += OnQuestChanged;
            QuestManager.OnQuestProgressUpdated += OnQuestChanged;
            QuestManager.OnQuestCompleted += OnQuestChanged;
        }
    }

    void CreateBubble()
    {
        currentBubble = Instantiate(questBubblePrefab, transform);
        currentBubble.transform.localPosition = bubbleOffset;
        bubbleSpriteRenderer = currentBubble.GetComponent<SpriteRenderer>();
    }

    void CreateDefaultBubble()
    {
        currentBubble = new GameObject("QuestBubble");
        currentBubble.transform.SetParent(transform);
        currentBubble.transform.localPosition = bubbleOffset;

        bubbleSpriteRenderer = currentBubble.AddComponent<SpriteRenderer>();
        bubbleSpriteRenderer.sortingLayerName = "UI";
        bubbleSpriteRenderer.sortingOrder = 100;
    }

    void OnQuestChanged(QuestData quest)
    {
        UpdateBubbleState();
    }

    void UpdateBubbleState()
    {
        if (currentBubble == null || QuestManager.Instance == null)
            return;

        bool shouldShow = false;
        Sprite bubbleSprite = null;

        if (HasAvailableQuest(npcName))
        {
            bubbleSprite = availableQuestSprite;
            shouldShow = true;
        }
        else if (IsInvolvedInActiveQuest(npcName))
        {
            bubbleSprite = inProgressQuestSprite;
            shouldShow = true;
        }

        currentBubble.SetActive(shouldShow);

        if (shouldShow && bubbleSprite != null && bubbleSpriteRenderer != null)
        {
            bubbleSpriteRenderer.sprite = bubbleSprite;
            StartBubbleAnimation();
        }
    }

    bool HasAvailableQuest(string npcName)
    {
        var allQuests = QuestManager.Instance.GetAllQuests();

        foreach (var quest in allQuests)
        {
            if (quest.giverNPC == npcName && quest.state == QuestState.LOCKED)
            {
                if (quest.requiredQuestIds.Count == 0) return true;

                bool prerequisitesMet = false;

                switch (quest.prerequisiteLogic)
                {
                    case PrerequisiteLogic.ANY:
                        foreach (var reqId in quest.requiredQuestIds)
                        {
                            var reqQuest = QuestManager.Instance.GetQuest(reqId);
                            if (reqQuest != null && reqQuest.state == QuestState.COMPLETED)
                            {
                                prerequisitesMet = true;
                                break;
                            }
                        }
                        break;

                    case PrerequisiteLogic.ALL:
                    default:
                        prerequisitesMet = true;
                        foreach (var reqId in quest.requiredQuestIds)
                        {
                            var reqQuest = QuestManager.Instance.GetQuest(reqId);
                            if (reqQuest == null || reqQuest.state != QuestState.COMPLETED)
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                        break;
                }

                if (prerequisitesMet) return true;
            }
        }

        return false;
    }

    bool IsInvolvedInActiveQuest(string npcName)
    {
        var activeQuests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in activeQuests)
        {
            if (quest.giverNPC == npcName)
            {
                return true;
            }

            foreach (var trigger in quest.completionTriggers)
            {
                if (trigger.triggerType == QuestTriggerType.TALK_TO_NPC &&
                    trigger.targetId == npcName)
                {
                    if (trigger.currentAmount < trigger.requiredAmount)
                    {
                        if (trigger.sequential)
                        {
                            bool previousComplete = true;
                            foreach (var otherTrigger in quest.completionTriggers)
                            {
                                if (otherTrigger == trigger) break;
                                if (otherTrigger.currentAmount < otherTrigger.requiredAmount)
                                {
                                    previousComplete = false;
                                    break;
                                }
                            }
                            return previousComplete;
                        }
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void StartBubbleAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(BubbleBobAnimation());
    }

    System.Collections.IEnumerator BubbleBobAnimation()
    {
        Vector3 startPos = bubbleOffset;
        float elapsedTime = 0f;
        float bobHeight = 0.2f;
        float bobSpeed = 2f;

        while (currentBubble != null && currentBubble.activeSelf)
        {
            elapsedTime += Time.deltaTime;
            float yOffset = Mathf.Sin(elapsedTime * bobSpeed) * bobHeight;
            currentBubble.transform.localPosition = startPos + new Vector3(0, yOffset, 0);

            yield return null;
        }

        if (currentBubble != null)
            currentBubble.transform.localPosition = startPos;
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.OnQuestStarted -= OnQuestChanged;
            QuestManager.OnQuestProgressUpdated -= OnQuestChanged;
            QuestManager.OnQuestCompleted -= OnQuestChanged;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + bubbleOffset);
    }
}