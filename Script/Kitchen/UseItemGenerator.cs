using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using SousRaccoon.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Kitchen
{
    public class UseItemGenerator : MonoBehaviour
    {
        public List<ItemDataBase> useItemSO = new();

        public Collider gereratorCollider;
        public ItemDataBase currentItem;
        public ItemDataBase handlerItem;
        public bool isHaveItem;

        [Header("Player")]
        public GameObject useItemPanel;
        public Image useItemIcon;

        public Transform buffLayout;
        public UIBuffNode buffPrefab;
        public UIBuffNode currentBuffObject;

        [Header("UI")]
        public GameObject canvasFinishItem;
        public GameObject ingredientReqGuide;
        public Image itemImage;
        public Sprite defaultImage;
        public TMP_Text countdownText;

        private float generateTime;
        public bool isGenerating;

        // Use This When Can Buy this

        /*
        private void Start()
        {
            if (GameManager.instance.playerSaveData.UnlockedDecorations[0] == 0)
            {
                useItemCanvas.SetActive(false);
                gereratorCollider.enabled = false;
                Destroy(this);
            }
        }*/

        //

        private void Start()
        {
            ingredientReqGuide.SetActive(true);
        }

        void Update()
        {
            if (isGenerating)
            {
                // ลดเวลาการนับถอยหลัง
                generateTime -= Time.deltaTime;

                // แสดงเวลาเป็นจำนวนเต็ม
                countdownText.text = Mathf.CeilToInt(generateTime).ToString();

                if (generateTime <= 0)
                {
                    // การสร้างไอเท็มเสร็จสิ้น
                    FinishGeneratingItem();
                }
            }
        }

        public void GetIngredient(ItemDataBase ingredient)
        {
            if (currentItem != null) return;

            ingredientReqGuide.SetActive(false);
            StageManager.instance.playerKitchenAction.CurrentItemCount--;
            StageManager.instance.playerKitchenAction.SetItem();
            StageManager.instance.playerKitchenAction.SetHoldingItem();

            foreach (var item in useItemSO)
            {
                if (item.useItem.ingredientToUse.IngredientName == ingredient.ingredient.IngredientName)
                {
                    currentItem = item;
                    GenerateItem();
                    return;
                }
            }

            Debug.LogWarning("Ingredient not found in useItemSO.");
        }

        public ItemDataBase GiveItem()
        {
            handlerItem = currentItem;
            itemImage.sprite = defaultImage;

            useItemIcon.gameObject.SetActive(true);
            useItemIcon.sprite = handlerItem.useItem.UseItemIcon;

            currentItem = null;
            isHaveItem = false;

            return handlerItem;
        }

        public void SpawnBuff()
        {
            useItemIcon.gameObject.SetActive(false);
            currentBuffObject = Instantiate(buffPrefab, buffLayout.position, Quaternion.identity, buffLayout);
            currentBuffObject.Icon.sprite = handlerItem.useItem.UseItemIcon;
        }

        public void OutPutBuff(float timer)
        {
            if (currentBuffObject != null)
                currentBuffObject.OutputBuff(timer);
        }

        public void FinishBuff()
        {
            Destroy(currentBuffObject.gameObject);
            currentBuffObject = null;
        }

        public void GenerateItem()
        {
            if (currentItem != null)
            {
                generateTime = currentItem.useItem.generateTime;
                countdownText.gameObject.SetActive(true);
                canvasFinishItem.SetActive(false);
                isGenerating = true;
            }
        }

        private void FinishGeneratingItem()
        {
            isGenerating = false;
            // ทำการแสดงไอเท็มและตั้งค่า UI เมื่อการนับถอยหลังเสร็จสิ้น
            countdownText.gameObject.SetActive(false);
            itemImage.sprite = currentItem.useItem.UseItemIcon;
            canvasFinishItem.SetActive(true);
            isHaveItem = true;
        }
    }
}