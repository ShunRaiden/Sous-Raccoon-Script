using SousRaccoon.UI;
using SousRaccoon.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UIBookPanel : MonoBehaviour
{
    //[SerializeField] UIUpgradeLevelPanel upgradeLevelPanel;
    [SerializeField] UIGuideBookPanel guideBookPanel;
    [SerializeField] UISkinShopPanel skinBookPanel;
    [SerializeField] GameObject upgradeContent;

    [SerializeField] Button upgradeButton;
    [SerializeField] Button skinShopButton;
    [SerializeField] Button guideButton;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnOpenUpgradeLevelPanel);
        skinShopButton.onClick.AddListener(OnOpenSkinShop);
        guideButton.onClick.AddListener(OnOpenGuideBookPanel);
    }

    public void OnOpenUpgradeLevelPanel()
    {
        upgradeContent.SetActive(true);
        skinBookPanel.OnClosePanel();
        guideBookPanel.ClosePanel();
    }

    public void OnOpenSkinShop()
    {
        skinBookPanel.OnOpenPanel();
        upgradeContent.SetActive(false);
        guideBookPanel.ClosePanel();
    }

    public void OnOpenGuideBookPanel()
    {
        guideBookPanel.OpenGuideBook();
        upgradeContent.SetActive(false);
        skinBookPanel.OnClosePanel();
    }
}
