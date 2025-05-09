using SousRaccoon.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace SousRaccoon.Manager
{
    public class DialogManager : MonoBehaviour
    {
        #region Singleton
        public static DialogManager instance { get { return _instance; } }
        private static DialogManager _instance;

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;//Avoid doing anything else
            }
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            _instance = this;
        }
        #endregion

        public Player.PlayerInputManager playerInputManager;

        public GameObject dialogUI;
        //public TMP_Text speakerNameText;
        public TextMeshProUGUI dialogText;
        public float typingSpeed = 0.05f;
        public DialogDataSO dialogData;
        public Image speakerImage;
        public Sprite playerSprite;

        private Queue<string> sentences;

        public PlayableDirector cutScene;

        [HideInInspector] public bool _isDialogueStart;
        [SerializeField] bool stopDialogue;
        bool conDialogue;

        [Header("Audio")]
        public bool stopAuido;
        AudioSource audioSource;
        AudioClip audioClip;

        [Range(1, 5)]
        [SerializeField] int frequencyOfSound = 2;

        void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        }

        void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(Locale newLocale)
        {
            if (_isDialogueStart)
            {
                dialogUI.SetActive(false);  // ปิด UI
                StopAllCoroutines(); // หยุด Coroutine ทุกตัวที่กำลังทำงานอยู่
                // เริ่มใหม่ทันทีหากกำลังแสดง Dialogue
                StartCoroutine(ReloadCurrentDialogue());
            }
        }

        void Start()
        {
            sentences = new Queue<string>();
            audioSource = AudioManager.instance.v_AudioSource;
            audioClip = AudioManager.instance.v_Dialouge;
        }

        private void Update()
        {
            if (!_isDialogueStart) return;

            if (Input.GetKeyDown(KeyCode.Space) && _isDialogueStart)
            {
                stopDialogue = true;
            }

            if (Input.GetKeyDown(KeyCode.Space) && !conDialogue)
            {
                conDialogue = true;
            }
        }

        #region Start Dialog
        /*public void StartDialogue(DialogDataSO dialogData)
        {
            _isDialogueStart = true;

            playerInputManager = FindObjectOfType<Player.PlayerInputManager>();
            playerInputManager.isCutScene = true;

            dialogUI.SetActive(true);
            this.dialogData = dialogData;

            //speakerNameText.text = speakerName;
            speakerImage.sprite = dialogData.speakerIcon;

            sentences.Clear();

            foreach (string sentence in this.dialogData.data)
            {
                sentences.Enqueue(sentence);
            }

            if (dialogData.haveCutScene)
                StartCoroutine(DisplayDialogCutScene(dialogData, dialogData.speakerName, dialogData.speakerIcon));
            else
                StartCoroutine(DisplayDialog(dialogData, dialogData.speakerName, dialogData.speakerIcon));
        }*/

        public void StartDialogue(DialogDataSO localizedDialogData)
        {
            dialogData = localizedDialogData;
            _isDialogueStart = true;
            playerInputManager = FindObjectOfType<Player.PlayerInputManager>();
            playerInputManager.isCutScene = true;

            dialogText.text = "";
            dialogUI.SetActive(true);

            sentences.Clear();

            // ดึงค่าภาษาปัจจุบันและเพิ่มเข้า Queue
            StartCoroutine(LoadLocalizedSentences(localizedDialogData));

            speakerImage.sprite = localizedDialogData.speakerIcon;
        }

        private IEnumerator LoadLocalizedSentences(DialogDataSO localizedDialogData)
        {
            foreach (var localizedString in localizedDialogData.dialogSentences)
            {
                var operation = localizedString.GetLocalizedStringAsync();
                yield return operation; // รอให้โหลดค่าเสร็จ
                sentences.Enqueue(operation.Result); // ใส่ข้อความที่ถูกแปลลง Queue
            }

            // เริ่มแสดงผล Dialogue หลังโหลดเสร็จ
            StartCoroutine(DisplayDialog());
        }

        #region Default Dialog
        IEnumerator DisplayDialog()
        {
            while (sentences.Count > 0)
            {
                stopDialogue = false;
                string sentence = sentences.Dequeue();

                dialogText.text = "";
                dialogText.maxVisibleCharacters = 0;

                PlayDialogueSound(dialogText.maxVisibleCharacters);

                foreach (char letter in sentence.ToCharArray())
                {
                    if (stopDialogue)
                    {
                        dialogText.text = sentence;
                        dialogText.maxVisibleCharacters = sentence.Length;
                        audioSource.Stop();
                        break;
                    }

                    dialogText.text += letter;
                    dialogText.maxVisibleCharacters++;
                    yield return new WaitForSeconds(typingSpeed);
                }

                yield return new WaitForSeconds(0.1f);
                audioSource.Stop();
                conDialogue = false;
                yield return new WaitUntil(() => conDialogue);
            }

            dialogUI.SetActive(false);
            playerInputManager.isCutScene = false;
            audioSource.Stop();
            _isDialogueStart = false;
        }

        IEnumerator DisplayDialog(DialogDataSO dialogData, string speakerName, Sprite speakerImg)
        {
            Queue<string> sentencesCopy = new Queue<string>(sentences);
            foreach (string sentence in sentencesCopy)
            {
                stopDialogue = false;

                dialogText.text = "";  // Clear the text before typing each sentence
                dialogText.maxVisibleCharacters = 0;

                PlayDialogueSound(dialogText.maxVisibleCharacters);

                foreach (char letter in sentence.ToCharArray())
                {
                    //New
                    if (stopDialogue)
                    {
                        dialogText.text = sentence;
                        dialogText.maxVisibleCharacters = sentence.Length;
                        audioSource.Stop();
                        break;
                    }

                    dialogText.text += letter;
                    dialogText.maxVisibleCharacters++;

                    yield return new WaitForSeconds(typingSpeed);

                }

                yield return new WaitForSeconds(0.1f);
                audioSource.Stop();
                conDialogue = false;
                yield return new WaitUntil(() => conDialogue);

                /*if (dialogData.switchChat && speakerName == speakerNameText.text)
                {
                    speakerNameText.text = dialogData.playerName;
                    speakerSprite.sprite = playerSprite;
                }
                else if (dialogData.switchChat)
                {
                    speakerNameText.text = speakerName;
                    speakerSprite.sprite = speakerImg;
                }*/


                yield return new WaitForSeconds(0.1f);
            }

            dialogUI.SetActive(false);
            playerInputManager.isCutScene = false;
            audioSource.Stop();
            _isDialogueStart = false;
            // Dialog is complete, you can add logic for when the conversation ends.
        }

        private IEnumerator ReloadCurrentDialogue()
        {
            // เก็บค่าข้อความที่กำลังแสดง
            DialogDataSO currentDialogData = dialogData;

            dialogText.text = "";  // ล้างข้อความที่แสดง
            dialogText.maxVisibleCharacters = 0;

            // หยุดการแสดงปัจจุบัน
            sentences.Clear();   // เคลียร์ประโยคที่ค้างอยู่

            audioSource.Stop();

            if (playerInputManager != null)
            {
                playerInputManager.isCutScene = true;
            }

            dialogUI.SetActive(true);

            // โหลดประโยคใหม่จากภาษาที่เปลี่ยน
            yield return StartCoroutine(LoadLocalizedSentences(currentDialogData));
        }
        #endregion

        #region AE Dialog
        IEnumerator DisplayDialogCutScene(DialogDataSO dialogData, string speakerName, Sprite speakerImg)
        {
            int index = 0;

            Queue<string> sentencesCopy = new Queue<string>(sentences);
            foreach (string sentence in sentencesCopy)
            {
                stopDialogue = false;

                dialogText.text = "";  // Clear the text before typing each sentence
                dialogText.maxVisibleCharacters = 0;
                foreach (char letter in sentence.ToCharArray())
                {
                    //New
                    if (stopDialogue)
                    {
                        dialogText.text = sentence;
                        dialogText.maxVisibleCharacters = sentence.Length;
                        break;
                    }

                    Debug.Log(dialogText.maxVisibleCharacters);
                    PlayDialogueSound(dialogText.maxVisibleCharacters);

                    dialogText.text += letter;
                    dialogText.maxVisibleCharacters++;

                    yield return new WaitForSeconds(typingSpeed);

                }

                yield return new WaitForSeconds(0.1f);
                conDialogue = false;
                yield return new WaitUntil(() => conDialogue);

                if (index == dialogData.cutSceneDialogIndex)
                {
                    cutScene.Play();

                    while (cutScene.state == PlayState.Playing)
                    {
                        yield return null;
                    }
                    Debug.Log("end");
                }

                index++;

                yield return new WaitForSeconds(0.1f);

                /*if (dialogData.switchChat && speakerName == speakerNameText.text)
                {
                    speakerNameText.text = dialogData.playerName;
                    speakerSprite.sprite = playerSprite;
                }
                else if (dialogData.switchChat)
                {
                    speakerNameText.text = speakerName;
                    speakerSprite.sprite = speakerImg;
                }*/


            }

            dialogUI.SetActive(false);

            yield return new WaitForSeconds(1f);
            StageManager.instance.playerInputManager.isCutScene = false;

            _isDialogueStart = false;
            // Dialog is complete, you can add logic for when the conversation ends.        
        }
        #endregion

        #region Sound
        private void PlayDialogueSound(int currentCharacterCount)
        {
            if (currentCharacterCount % frequencyOfSound == 0)
            {
                if (audioSource == null)
                {
                    audioSource = AudioManager.instance.v_AudioSource;
                }

                if (stopAuido)
                {
                    audioSource.Stop();
                }

                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
        #endregion

        #endregion

        public void StopAllDialogue()
        {
            StopAllCoroutines(); // หยุด Coroutine ทุกตัวที่กำลังทำงานอยู่
            sentences.Clear();   // เคลียร์ประโยคที่ค้างอยู่

            dialogText.text = "";  // ล้างข้อความที่แสดง
            dialogText.maxVisibleCharacters = 0;

            audioSource.Stop();

            dialogUI.SetActive(false);  // ปิด UI
            _isDialogueStart = false;

            if (playerInputManager != null)
            {
                playerInputManager.isCutScene = false;
            }
        }

    }
}