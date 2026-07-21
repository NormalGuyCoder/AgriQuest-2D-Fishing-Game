using UnityEngine;
using UnityEngine.UI;

public class BoneButton : MonoBehaviour
{
    private BoneData boneData;
    private FishDefinition fishDefinition;
    private Button button;
    private Image image;

    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }

        // Make it transparent but clickable
       // image.color = new Color(1f, 1f, 1f, 0.3f);
    }

    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Initialize(BoneData bone, FishDefinition fish)
    {
        boneData = bone;
        fishDefinition = fish;
    }

    private void OnButtonClicked()
    {
        if (boneData != null && !boneData.isRemoved && GameManager.Instance != null)
        {
            GameManager.Instance.OnBoneClicked(boneData, this);
        }
    }

    public void SetRemoved(bool removed)
    {
        if (image != null)
        {
            if (removed)
            {
                image.color = new Color(0.5f, 0.5f, 0.5f, 0.1f); // Dimmed
                if (button != null)
                    button.interactable = false;
            }
            else
            {
                image.color = new Color(1f, 1f, 1f, 0.3f); // Normal
                if (button != null)
                    button.interactable = true;
            }
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}




