using UnityEngine;

public class DialogInteractor : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Professor";
    public string startingDialogId = "orientation_lecture_part1";

    [Header("Interaction Settings")]
    public float interactionRadius = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Shop Settings")]
    public bool hasShop = false;
    public SellShopInteractor shopInteractor;

    [Header("UI Indicator")]
    public GameObject interactionIndicator;

    private bool playerInRange = false;

    void Start()
    {
        var collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            var circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = interactionRadius;
            circleCollider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = true;
        }

        if (hasShop && shopInteractor == null)
        {
            shopInteractor = GetComponent<SellShopInteractor>();
        }

        // Hide indicator at start
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // NEW: Don't allow interaction if dialogue is already active
        if (DialogSystem.Instance != null && DialogSystem.Instance.IsDialogActive())
            return;
        
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Show the interaction indicator
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(true);
            }

            if (hasShop && shopInteractor != null)
            {
                shopInteractor.SetPlayerInRange(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide the interaction indicator
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(false);
            }

            if (hasShop && shopInteractor != null)
            {
                shopInteractor.SetPlayerInRange(false);
            }
            
            // MODIFIED: Only end dialogue if player walks far away
            if (DialogSystem.Instance != null &&
                DialogSystem.Instance.IsDialogActive() &&
                DialogSystem.Instance.GetCurrentNPC() == npcName)
            {
                float distance = Vector2.Distance(transform.position, other.transform.position);
                if (distance > interactionRadius * 1.5f)
                {
                    DialogSystem.Instance.EndDialog();
                }
            }
        }
    }

    public void Interact()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RecordTalkedToNPC(npcName);
        }

        if (DialogSystem.Instance != null)
        {
            // MODIFIED: Smart dialog selection
            string dialogIdToUse = startingDialogId;
            
            // If player has already heard the orientation lecture, don't show it again
            if (npcName == "Professor Clark" && 
                !string.IsNullOrEmpty(startingDialogId) && 
                startingDialogId == "orientation_lecture_part1")
            {
                // Check if orientation quest is completed
                var quest = QuestManager.Instance?.GetQuest("ORIENTATION_DAY_0");
                if (quest != null && quest.state == QuestState.COMPLETED)
                {
                    // Don't force the orientation lecture - let DialogSystem find appropriate dialog
                    dialogIdToUse = "";
                    Debug.Log("Professor Clark: Orientation already completed, finding appropriate dialog");
                }
            }
            
            bool success = DialogSystem.Instance.StartDialog(npcName, dialogIdToUse);

            if (!success)
            {
                success = DialogSystem.Instance.StartDialog(npcName, "");
            }
        }
        else
        {
            Debug.LogError("DialogInteractor: DialogManager not found!");
        }
    }

    public void OpenShop()
    {
        if (hasShop && shopInteractor != null)
        {
            shopInteractor.OpenShopUI();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}