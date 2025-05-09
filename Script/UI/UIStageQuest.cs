using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace SousRaccoon.UI
{
    public class UIStageQuest : MonoBehaviour
    {
        public TMP_Text headerText;
        public TMP_Text progressText;

        public LocalizeStringEvent headerLocalized;

        Animator animator;

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void SetUp(string header, string progress)
        {
            headerText.text = header;
            progressText.text = progress;
        }

        public void SetUpLocalized(LocalizedString localizedString)
        {
            headerLocalized.StringReference = localizedString;
        }

        public void FinishQuest()
        {
            animator.Play("FadeOut");
            Destroy(gameObject, 2f);
        }
    }
}