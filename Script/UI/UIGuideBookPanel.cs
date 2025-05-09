using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UIGuideBookPanel : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] GameObject content;
        [SerializeField] GameObject mainGuideBook;
        [SerializeField] Button backButton;

        [Header("Monster")]
        [SerializeField] GameObject monsterPanel;
        [SerializeField] List<GameObject> monsterPage;

        [Header("Scenario")]
        [SerializeField] GameObject scenarioPanel;
        [SerializeField] List<GameObject> scenarioPage;

        [Header("Debuff")]
        [SerializeField] GameObject debuffPanel;

        [Header("Perk")]
        [SerializeField] GameObject perkPanel;
        [SerializeField] List<GameObject> perkPage;

        private void Start()
        {
            backButton.onClick.AddListener(OpenGuideBook);
        }

        private void OnDestroy()
        {
            backButton.onClick.RemoveAllListeners();
        }

        public void OpenGuideBook()
        {
            CloseAllPanel();
            content.SetActive(true);
            mainGuideBook.SetActive(true);
        }

        public void OpenMonsterPage()
        {
            CloseAllPanel();
            monsterPanel.SetActive(true);
            ResetPage(monsterPage);
        }

        public void OpenScenarioPage()
        {
            CloseAllPanel();
            scenarioPanel.SetActive(true);
            ResetPage(scenarioPage);
        }

        public void OpenDebuffPage()
        {
            CloseAllPanel();
            debuffPanel.SetActive(true);
        }

        public void OpenPerkPage()
        {
            CloseAllPanel();
            perkPanel.SetActive(true);
            ResetPage(perkPage);
        }

        public void ClosePanel()
        {
            content.SetActive(false);
            CloseAllPanel();
        }

        public void CloseAllPanel()
        {
            mainGuideBook.SetActive(false);
            monsterPanel.SetActive(false);
            scenarioPanel.SetActive(false);
            debuffPanel.SetActive(false);
            perkPanel.SetActive(false);
        }

        void ResetPage(List<GameObject> pages)
        {
            if (pages == null || pages.Count == 0) return;

            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(false);
            }

            pages[0].SetActive(true); // เปิดหน้าหลัก
        }
    }
}
