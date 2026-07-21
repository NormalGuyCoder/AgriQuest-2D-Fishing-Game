using UnityEngine;
using System.Collections;

public class SellShopInteractor : MonoBehaviour
{
    public GameObject sellShopUIGameObject;
    public SellShopUI sellShopUIComponent;

    private bool playerInRange = false;

    void Start()
    {
        if (sellShopUIGameObject != null)
        {
            sellShopUIGameObject.SetActive(false);
        }

        if (sellShopUIComponent == null && sellShopUIGameObject != null)
        {
            sellShopUIComponent = sellShopUIGameObject.GetComponent<SellShopUI>();
            if (sellShopUIComponent == null)
            {
                sellShopUIComponent = sellShopUIGameObject.GetComponentInChildren<SellShopUI>();
            }
        }
    }


    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
        if (!inRange)
        {
            CloseShopUI();
        }
    }

    public void ToggleShopUI()
    {
        if (sellShopUIGameObject != null)
        {
            bool shouldActivate = !sellShopUIGameObject.activeSelf;
            sellShopUIGameObject.SetActive(shouldActivate);

            if (shouldActivate)
            {
                StartCoroutine(RefreshShopAfterDelay());
            }
        }
    }

    public void OpenShopUI()
    {
        if (sellShopUIGameObject != null && !sellShopUIGameObject.activeSelf)
        {
            sellShopUIGameObject.SetActive(true);
            StartCoroutine(RefreshShopAfterDelay());
        }
    }

    private IEnumerator RefreshShopAfterDelay()
    {
        yield return new WaitForEndOfFrame();

        if (sellShopUIComponent != null)
        {
            sellShopUIComponent.RefreshFishList();
        }
        else
        {
            if (sellShopUIGameObject != null)
            {
                sellShopUIComponent = sellShopUIGameObject.GetComponent<SellShopUI>();
                if (sellShopUIComponent == null)
                {
                    sellShopUIComponent = sellShopUIGameObject.GetComponentInChildren<SellShopUI>();
                }

                if (sellShopUIComponent != null)
                {
                    sellShopUIComponent.RefreshFishList();
                }
            }
        }
    }

    public void CloseShopUI()
    {
        if (sellShopUIGameObject != null && sellShopUIGameObject.activeSelf)
        {
            sellShopUIGameObject.SetActive(false);
        }
    }

    public void CloseShop()
    {
        CloseShopUI();
    }
}