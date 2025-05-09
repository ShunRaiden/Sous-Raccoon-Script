using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class SelectionStageManager : MonoBehaviour
    {
        [SerializeField] GameObject selectionPanel;

        [SerializeField] List<GameObject> stageButtonList;

        [SerializeField] Animator anim;

        Coroutine panelCoroutine;

        public void SetUpPanel()
        {
            StopCoroutine(OnClosePanel());

            selectionPanel.SetActive(true);
            anim.Play("Open");

            if (GameManager.instance.isDemo)
                stageButtonList[0].SetActive(true);
            else
                CheckStageUnlock();
        }

        public void ClosePanel()
        {
            if (panelCoroutine != null) return;

            panelCoroutine = StartCoroutine(OnClosePanel());
        }

        IEnumerator OnClosePanel()
        {
            anim.Play("Close");
            yield return new WaitForSeconds(0.66f);
            selectionPanel.SetActive(false);

            panelCoroutine = null;
        }

        private void CheckStageUnlock()
        {
            for (int i = 0; i <= GameManager.instance.playerSaveData.StageUnlock; i++)
            {
                if (i < stageButtonList.Count)
                    stageButtonList[i].SetActive(true);
            }
        }
    }
}
