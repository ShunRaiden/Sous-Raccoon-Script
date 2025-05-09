using SousRaccoon.Customer;
using SousRaccoon.Data.Item;
using SousRaccoon.Kitchen;
using SousRaccoon.Lobby;
using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SousRaccoon.Data.Item.ItemDataBase;
using static SousRaccoon.Data.Item.OrderSO;

namespace SousRaccoon.Player
{
    public class PlayerKitchenAction : MonoBehaviour
    {
        PlayerBuffManager playerBuffManager;
        PlayerCombatSystem playerCombatSystem;
        PlayerHealingDanceSystem playerHealingDanceSystem;
        PlayerLocomotion playerLocomotion;
        PlayerAnimatorManager animManager;
        PlayerInputManager inputManager;
        KitchenTable kitchenTable;
        BlenderTable blenderTable;
        BlenderMaker blenderMaker;
        ChefAI chef;

        [Header("Item")]
        public ItemDataBase itemDataBase;
        public ItemDataBase itemFirstDataBase;
        public ItemDataBase itemSecondDataBase;

        public ItemDataBase CurrentItem
        {
            get
            {
                if (!useTwoHand) return itemDataBase;
                return isUsingFirstHand ? itemFirstDataBase : itemSecondDataBase;
            }
            set
            {
                if (!useTwoHand) itemDataBase = value;
                else
                {
                    if (isUsingFirstHand) itemFirstDataBase = value;
                    else itemSecondDataBase = value;
                }
            }
        }

        public int itemCount;
        public int itemFirstCount;
        public int itemSecondCount;

        public int CurrentItemCount
        {
            get
            {
                if (!useTwoHand) return itemCount;
                return isUsingFirstHand ? itemFirstCount : itemSecondCount;
            }
            set
            {
                if (!useTwoHand) itemCount = value;
                else
                {
                    if (isUsingFirstHand) itemFirstCount = value;
                    else itemSecondCount = value;
                }
            }
        }

        public bool useTwoHand;
        public bool isUsingFirstHand;

        [Header("Item UI One Hand")]
        [SerializeField] private GameObject itemBG;
        [SerializeField] private UnityEngine.UI.Image itemImage;
        [SerializeField] private TMP_Text itemCountText;
        [SerializeField] private GameObject orderIcon;
        [SerializeField] private GameObject waterOrderIcon;

        [Header("Item Two Hand")]
        [SerializeField] private GameObject twoHandBG;
        [SerializeField] private Animator twoHandAnim;

        [Header("Item UI First Hand")]
        [SerializeField] private GameObject itemFirstBG;
        [SerializeField] private UnityEngine.UI.Image itemFirstImage;
        [SerializeField] private TMP_Text itemCountFirstText;
        [SerializeField] private GameObject orderFirstIcon;
        [SerializeField] private GameObject waterOrderFirstIcon;

        [Header("Item UI Second Hand")]
        [SerializeField] private GameObject itemSecondBG;
        [SerializeField] private UnityEngine.UI.Image itemSecondImage;
        [SerializeField] private TMP_Text itemCountSecondText;
        [SerializeField] private GameObject orderSecondIcon;
        [SerializeField] private GameObject waterOrderSecondIcon;

        [Header("Item Spawn")]
        [SerializeField] private Transform itemSpawnPoint;
        [SerializeField] private GameObject itemHolder;

        [Header("UseItem")]
        [SerializeField] private ItemDataBase useItemDataBase;

        [SerializeField] private Collider currentTarget = null;   // เก็บเป้าหมายที่ใกล้ที่สุดในปัจจุบัน
        [SerializeField] private Collider previousTarget = null;  // เก็บเป้าหมายที่ตรวจจับก่อนหน้า
        private List<Collider> targetsInRange = new List<Collider>();

        bool repairing = false;
        bool cleaning = false;

        private void Start()
        {
            playerBuffManager = GetComponent<PlayerBuffManager>();
            playerCombatSystem = GetComponent<PlayerCombatSystem>();
            playerHealingDanceSystem = GetComponent<PlayerHealingDanceSystem>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            animManager = GetComponent<PlayerAnimatorManager>();
            inputManager = GetComponent<PlayerInputManager>();
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            blenderTable = FindAnyObjectByType<BlenderTable>();
            blenderMaker = FindAnyObjectByType<BlenderMaker>();
            chef = FindAnyObjectByType<ChefAI>();
        }
        private void Update()
        {
            // อัปเดตเป้าหมายที่ใกล้ที่สุดใหม่ตลอดเวลา
            currentTarget = GetClosestTarget();

            // ถ้าเป้าหมายที่ใกล้ที่สุดเปลี่ยนแปลง ให้ทำการแสดง/ซ่อน Outliner
            if (currentTarget != previousTarget)
            {
                // ซ่อน Outliner ของเป้าหมายก่อนหน้า (ถ้ามี)
                if (previousTarget != null)
                {
                    HideHighlight(previousTarget);
                }

                // แสดง Outliner ของเป้าหมายปัจจุบัน (ถ้ามี)
                if (currentTarget != null)
                {
                    ShowHighlight(currentTarget);
                }

                // อัปเดต previousTarget ให้เป็นเป้าหมายปัจจุบัน
                previousTarget = currentTarget;
            }
        }

        public void PerformInteraction()
        {
            if (targetsInRange.Count == 0) return;

            Collider closestTarget = GetClosestTarget();

            if (closestTarget != null)
            {
                Debug.Log("Interacting with: " + closestTarget.name);

                if (closestTarget.CompareTag("KitchenTable"))
                {
                    if (CurrentItem.itemType == ItemDataBase.ItemType.Order) GiveOrderKT();
                }
                else if (closestTarget.CompareTag("BlenderTable"))
                {
                    if (CurrentItem.itemType == ItemDataBase.ItemType.Order) GiveOrderBT();
                }
                else if (closestTarget.CompareTag("Table"))
                {
                    var table = closestTarget.GetComponent<Kitchen.Table>();

                    if (table == null || table.customerMenuDics.Count <= 0) return;

                    if (useTwoHand)
                    {
                        HandleFoodAndOrder(table);
                    }
                    else
                    {
                        if (CurrentItem == null || CurrentItem.itemType == ItemDataBase.ItemType.Order) GetOrder(table);

                        if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Food) GiveFoodToCustomer(table);
                    }
                }
                else if (closestTarget.CompareTag("FoodTable"))
                {
                    var foodTable = closestTarget.GetComponent<FoodTable>();

                    if (foodTable == null) { Debug.LogError($"foodTable is Null"); return; }

                    if (foodTable.foodData != null)
                    {
                        if (CanReceiveItem(CurrentItem, ItemDataBase.ItemType.Food)) GetFood(foodTable);
                    }
                }
                else if (closestTarget.CompareTag("ReturnFoodPoint"))
                {
                    var returnPoint = closestTarget.GetComponent<ReturnPoint>();

                    if (returnPoint == null) { Debug.LogError($"Return Point is Null"); return; }

                    if (
                        (returnPoint.type == ReturnPoint.ReturnPointType.kitchen && CurrentItem.food.TypeFood == FoodSO.FoodType.Food)
                        ||
                        (returnPoint.type == ReturnPoint.ReturnPointType.blender && CurrentItem.food.TypeFood == FoodSO.FoodType.Drink)
                       )
                    {
                        if (CurrentItem != null) GiveFoodToReturnPoint(returnPoint);
                    }
                }
                else if (closestTarget.CompareTag("IngredientBox"))
                {
                    Debug.LogWarning("Get IGD");
                    var box = closestTarget.GetComponent<IngredientBox>();
                    if (box == null) return;

                    if (CanReceiveItem(CurrentItem, ItemDataBase.ItemType.Ingredient)) GetIngredient(box);
                }
                else if (closestTarget.CompareTag("Chef"))
                {
                    if (StageManager.instance.isGameWin) { GetEndDay(); return; }

                    if (!StageManager.instance.isGameStart) GetGameStarted();

                    if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) GiveIngredientChef();
                }
                else if (closestTarget.CompareTag("BlenderMaker"))
                {
                    if (StageManager.instance.isGameWin) { return; }

                    if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) GiveIngredientBlender();
                }
                else if (closestTarget.CompareTag("DeadNPC"))
                {
                    var npc = closestTarget.GetComponent<CustomerDead>();
                    if (npc == null || !inputManager.isInteractAction) return;

                    animManager.CleanUp(inputManager.isInteractAction);//Animation Clean Up
                    StartCleanUp(npc);
                }
                else if (closestTarget.CompareTag("Trash"))
                {
                    var cTrash = closestTarget.GetComponent<CustomerTrash>();

                    if (cTrash == null) return;

                    if (CanReceiveItem(CurrentItem, ItemDataBase.ItemType.Ingredient)) GetTrash(cTrash);
                }
                else if (closestTarget.CompareTag("TrashCan"))
                {
                    var trashCan = closestTarget.GetComponent<TrashCan>();

                    if (trashCan == null || CurrentItem == null) return;

                    if ((CurrentItem.itemType == ItemDataBase.ItemType.Trash || CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) && trashCan.isOutSide) GiveTrash(); //Only Outside

                    if (CurrentItem.itemType == ItemDataBase.ItemType.Ingredient && !trashCan.isOutSide)
                    {
                        //GiveIngredientToTrashCan(); // = -1

                        GiveTrash();// = 0

                        trashCan.PlayAnimation();
                    }
                }
                else if (closestTarget.CompareTag("ItemGenerator"))
                {
                    var itemGenerator = closestTarget.GetComponent<UseItemGenerator>();

                    if (itemGenerator == null) return;

                    if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient && itemGenerator.currentItem == null)
                    {
                        GiveIngredientToItemGenerator(itemGenerator);
                        return;
                    }
                    else if (useItemDataBase == null && itemGenerator.isHaveItem)
                    {
                        var item = itemGenerator.GiveItem();
                        GetUseItem(item);
                    }
                }
                else if (closestTarget.CompareTag("Enemy"))
                {
                    var monsterGiver = closestTarget.GetComponent<MonsterGiverStatus>();

                    if (monsterGiver == null) return;

                    if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient && monsterGiver.canTakeIGD)
                    {
                        GiveIngredientToEnemy(monsterGiver);
                    }
                }
                else if (closestTarget.CompareTag("Merchant"))
                {
                    var shopMerchant = closestTarget.GetComponent<MerchantAI>();

                    if (shopMerchant == null || shopMerchant.shopMerchantManager == null || !shopMerchant.canOpenShop) return;

                    GetOpenShop(shopMerchant.shopMerchantManager);
                }
                else if (closestTarget.CompareTag("Barricade"))
                {
                    var barricade = closestTarget.GetComponent<BarricadeStatus>();
                    if (barricade == null || !inputManager.isInteractAction) return;

                    animManager.CleanUp(inputManager.isInteractAction);//Animation Clean Up
                    StartRepairing(barricade);
                }
                else if (closestTarget.CompareTag("LobbyBook"))
                {
                    var book = closestTarget.GetComponent<LobbyBook>();

                    if (book == null) return;

                    book.OpenLobbyPanel(inputManager, true);
                }
                else if (closestTarget.CompareTag("LobbyInteraction"))
                {
                    var interactItem = closestTarget.GetComponent<InteractionLobby>();

                    if (interactItem == null) return;

                    interactItem.OnStartInteraction();
                }
            }

            SetItem();
            SetHoldingItem();
        }

        public void PerformToggleHand()
        {
            UpdateActiveHandUI(); // ฟังก์ชันไว้แสดงว่ากำลังใช้มือไหน
        }

        public void PerformUseItem()
        {
            if (useItemDataBase == null) return;

            switch (useItemDataBase.useItem.buffType)
            {
                case UseItemSO.BuffType.Speed:
                    playerBuffManager.ApplyBuff(UseItemSO.BuffType.Speed, useItemDataBase.useItem.BuffTime, 0, useItemDataBase.useItem.BuffStatfloat);
                    break;
                case UseItemSO.BuffType.AttackSpeed:
                    playerBuffManager.ApplyBuff(UseItemSO.BuffType.AttackSpeed, useItemDataBase.useItem.BuffTime, 0, useItemDataBase.useItem.BuffStatfloat);
                    break;
                case UseItemSO.BuffType.AttackDamage:
                    playerBuffManager.ApplyBuff(UseItemSO.BuffType.AttackDamage, useItemDataBase.useItem.BuffTime, useItemDataBase.useItem.BuffStatint, 0);
                    break;
                case UseItemSO.BuffType.HealRange:
                    playerBuffManager.ApplyBuff(UseItemSO.BuffType.HealRange, useItemDataBase.useItem.BuffTime, 0, useItemDataBase.useItem.BuffStatfloat);
                    break;
                case UseItemSO.BuffType.HealRate:
                    playerBuffManager.ApplyBuff(UseItemSO.BuffType.HealRate, useItemDataBase.useItem.BuffTime, 0, useItemDataBase.useItem.BuffStatfloat);
                    break;
            }

            useItemDataBase = null;
        }

        private Collider GetClosestTarget()
        {
            Collider closest = null;
            float minDistance = Mathf.Infinity;

            // ใช้ for loop แทน foreach เพื่อให้สามารถลบ target ที่ถูกทำลายได้
            for (int i = targetsInRange.Count - 1; i >= 0; i--)
            {
                Collider target = targetsInRange[i];

                // ตรวจสอบว่า target ยังไม่ถูกทำลาย
                if (target == null || !target.enabled)
                {
                    // หาก target ถูกทำลายไปแล้ว ให้ลบออกจาก List
                    targetsInRange.RemoveAt(i);
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsDetectable(other))
            {
                if (!targetsInRange.Contains(other))
                {
                    targetsInRange.Add(other);
                    Debug.Log("Entered trigger of: " + other.name);
                }
            }

            if (other.CompareTag("Table"))
            {
                var table = other.gameObject.GetComponent<Kitchen.Table>();

                if (table == null) return;

                if (table.TryGetHasCustomer())
                {
                    if (!targetsInRange.Contains(other))
                    {
                        targetsInRange.Add(other);
                        Debug.Log("Entered trigger of: " + other.name);
                    }
                }
            }

            if (other.CompareTag("Barricade"))
            {
                var barricade = other.gameObject.GetComponent<BarricadeStatus>();

                if (barricade == null) return;

                if (barricade.canRepair)
                {
                    if (!targetsInRange.Contains(other))
                    {
                        targetsInRange.Add(other);
                        Debug.Log("Entered trigger of: " + other.name);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (targetsInRange.Contains(other))
            {
                targetsInRange.Remove(other);
                Debug.Log("Exited trigger of: " + other.name);

                // ซ่อน Outliner ของเป้าหมายที่ออกจากระยะ
                if (other == currentTarget)
                {
                    HideHighlight(other);
                    currentTarget = null;  // เคลียร์ currentTarget เมื่อตัวปัจจุบันออกจากระยะ
                }
            }
        }

        // ฟังก์ชันช่วยในการตรวจสอบชนิดของเป้าหมายที่เราต้องการตรวจจับ
        private bool IsDetectable(Collider other)
        {
            if (useTwoHand)
            {
                return other.CompareTag("Chef") || other.CompareTag("Merchant") ||
                   other.CompareTag("FoodTable") || other.CompareTag("DeadNPC") ||
                   other.CompareTag("ItemGenerator") || other.CompareTag("LobbyBook") || other.CompareTag("LobbyInteraction") ||
                   other.CompareTag("Trash") ||
                   other.CompareTag("IngredientBox") ||
                   (other.CompareTag("Enemy") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) ||
                   (other.CompareTag("KitchenTable") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Order && CurrentItem.order.type == OrderType.Food) ||
                   (other.CompareTag("ReturnFoodPoint") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Food) ||
                   (other.CompareTag("BlenderTable") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Order && CurrentItem.order.type == OrderType.Drink) ||
                   (other.CompareTag("BlenderMaker") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) ||
                   (other.CompareTag("TrashCan") && CurrentItem != null && (CurrentItem.itemType == ItemDataBase.ItemType.Ingredient || CurrentItem.itemType == ItemDataBase.ItemType.Trash));
            }

            return other.CompareTag("Chef") || other.CompareTag("Merchant") ||
                   other.CompareTag("FoodTable") || other.CompareTag("DeadNPC") ||
                   other.CompareTag("ItemGenerator") || other.CompareTag("LobbyBook") || other.CompareTag("LobbyInteraction") ||
                   (other.CompareTag("Trash") && (CurrentItem == null || CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Trash)) ||
                   (other.CompareTag("IngredientBox") && (CurrentItem == null || (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient))) ||
                   (other.CompareTag("Enemy") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) ||
                   (other.CompareTag("KitchenTable") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Order && CurrentItem.order.type == OrderType.Food) ||
                   (other.CompareTag("ReturnFoodPoint") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Food) ||
                   (other.CompareTag("BlenderTable") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Order && CurrentItem.order.type == OrderType.Drink) ||
                   (other.CompareTag("BlenderMaker") && CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Ingredient) ||
                   (other.CompareTag("TrashCan") && CurrentItem != null && (CurrentItem.itemType == ItemDataBase.ItemType.Ingredient || CurrentItem.itemType == ItemDataBase.ItemType.Trash));
        }

        private bool CanReceiveItem(ItemDataBase newItem, ItemType typeOfItem)
        {
            if (!useTwoHand)
            {
                return newItem == null || newItem.itemType == typeOfItem;
            }

            ItemDataBase otherHandItem = isUsingFirstHand ? itemSecondDataBase : itemFirstDataBase;

            if (newItem == null || newItem.itemType == typeOfItem)
            {
                return true; // มือปัจจุบันว่างหรือชนิดตรงกัน
            }

            if (otherHandItem == null || otherHandItem.itemType == typeOfItem)
            {
                PerformToggleHand(); // สลับมือเพื่อรับของ
                return true;
            }

            return false; // ทั้งสองมือมีของชนิดอื่น
        }

        private void HandleFoodAndOrder(Kitchen.Table table)
        {
            var currentHandItem = isUsingFirstHand ? itemFirstDataBase : itemSecondDataBase;
            var otherHandItem = isUsingFirstHand ? itemSecondDataBase : itemFirstDataBase;

            Debug.Log($"[Start] HandleFoodAndOrder: isUsingFirstHand = {isUsingFirstHand}, currentHandItem = {currentHandItem?.itemName}, otherHandItem = {otherHandItem?.itemName}, CurrentItem = {CurrentItem?.itemName}");

            // 1. มีอาหารในมือ -> ให้ลูกค้า
            if (CurrentItem != null && CurrentItem.itemType == ItemDataBase.ItemType.Food)
            {
                Debug.Log("[Step1] มีอาหารในมือ -> ให้ลูกค้า");
                GiveFoodToCustomer(table);
                return;
            }

            // 2. มือปัจจุบันถือของ -> ตรงกับ Order ไหม
            if (currentHandItem != null && currentHandItem.itemType == ItemType.Order)
            {
                var match = table.GetItemDataMatchingPlayer(currentHandItem);
                Debug.Log($"[Step2] currentHandItem: {currentHandItem.itemName}, match: {match?.itemName}");

                if (match != null && CheckNameItem(currentHandItem, match.itemName))
                {
                    Debug.Log("[Step2.1] currentHandItem ตรงกับ Order -> ตั้งเป็น CurrentItem และโชว์ UI");
                    CurrentItem = currentHandItem;
                    CurrentItemCount++;
                    ShowOrderIcon(match.order.type);
                    return;
                }
            }

            // 3. มืออีกข้างถือของ -> ตรงกับ Order ไหม
            if (otherHandItem != null && otherHandItem.itemType == ItemType.Order)
            {
                var matchOther = table.GetItemDataMatchingPlayer(otherHandItem);
                Debug.Log($"[Step3] otherHandItem: {otherHandItem.itemName}, matchOther: {matchOther?.itemName}");

                if (matchOther != null && CheckNameItem(otherHandItem, matchOther.itemName))
                {
                    Debug.Log("[Step3.1] otherHandItem ตรงกับ Order -> สลับมือ, ตั้งเป็น CurrentItem และโชว์ UI");
                    PerformToggleHand();
                    CurrentItem = otherHandItem;
                    CurrentItemCount++;
                    ShowOrderIcon(matchOther.order.type);
                    return;
                }
            }

            // 4. ไม่มีของในมือเลย -> หยิบ Order อันที่ใกล้หมดที่สุด
            if (currentHandItem == null)
            {
                Debug.Log("[Step4] มือทั้งสองว่าง -> หาของใหม่จากโต๊ะ");
                var newOrder = table.GetItemDataWithLowestHealthTime(null);
                if (newOrder != null)
                {
                    CurrentItem = newOrder;
                    CurrentItemCount++;
                    ShowOrderIcon(newOrder.order.type);
                    return;
                }
            }

            // 5. มือที่ใช้อยู่มีของ แต่อีกข้างว่าง -> สลับมือแล้วหยิบ Order
            if (otherHandItem == null)
            {
                Debug.Log("[Step5] มืออีกข้างว่าง -> สลับมือแล้วหยิบ Order ใหม่");
                var newOrderSecond = table.GetItemDataWithLowestHealthTime(null);
                if (newOrderSecond != null)
                {
                    PerformToggleHand();
                    CurrentItem = newOrderSecond;
                    CurrentItemCount++;
                    ShowOrderIcon(newOrderSecond.order.type);
                    return;
                }
            }
        }


        private bool CheckNameItem(ItemDataBase newItem, string nameItem)
        {
            if (!useTwoHand)
            {
                return newItem == null || newItem.itemName == nameItem;
            }

            ItemDataBase otherHandItem = isUsingFirstHand ? itemSecondDataBase : itemFirstDataBase;

            if (newItem == null || newItem.itemName == nameItem)
            {
                return true; // มือปัจจุบันว่างหรือชนิดตรงกัน
            }

            if (otherHandItem == null || otherHandItem.itemName == nameItem)
            {
                PerformToggleHand(); // สลับมือเพื่อรับของ
                return true;
            }

            return false; // ทั้งสองมือมีของชนิดอื่น
        }

        #region Set
        public void SetItem()
        {
            if (CurrentItemCount <= 0)
            {
                CurrentItem = null;
                CurrentItemCount = 0;
                if (!useTwoHand)
                {
                    itemBG.SetActive(false);
                    orderIcon.SetActive(false);
                    waterOrderIcon.SetActive(false);
                }
                else
                {
                    twoHandBG.SetActive(true);
                    if (isUsingFirstHand)
                    {
                        itemFirstImage.gameObject.SetActive(false);
                        itemCountFirstText.gameObject.SetActive(false);
                        orderFirstIcon.SetActive(false);
                        waterOrderFirstIcon.SetActive(false);

                        if (itemSecondCount <= 0)
                        {
                            itemFirstBG.SetActive(false);
                            itemSecondBG.SetActive(false);
                        }
                    }
                    else
                    {
                        itemSecondImage.gameObject.SetActive(false);
                        itemCountSecondText.gameObject.SetActive(false);
                        orderSecondIcon.SetActive(false);
                        waterOrderSecondIcon.SetActive(false);

                        if (itemFirstCount <= 0)
                        {
                            itemFirstBG.SetActive(false);
                            itemSecondBG.SetActive(false);
                        }
                    }
                    PerformToggleHand();
                }
                return;
            }

            if (CurrentItem == null) return;

            CurrentItem.GetItemIcon(out var icon);

            if (!useTwoHand)
            {
                itemImage.sprite = icon;
                itemCountText.text = CurrentItemCount.ToString();
                itemBG.SetActive(true);
            }
            else
            {
                twoHandBG.SetActive(true);
                if (isUsingFirstHand)
                {
                    itemFirstImage.sprite = icon;
                    itemCountFirstText.text = CurrentItemCount.ToString();
                    itemCountFirstText.gameObject.SetActive(true);
                    itemFirstImage.gameObject.SetActive(true);
                    itemFirstBG.SetActive(true);
                    itemSecondBG.SetActive(true);
                }
                else
                {
                    itemSecondImage.sprite = icon;
                    itemCountSecondText.text = CurrentItemCount.ToString();
                    itemSecondImage.gameObject.SetActive(true);
                    itemCountSecondText.gameObject.SetActive(true);
                    itemFirstBG.SetActive(true);
                    itemSecondBG.SetActive(true);
                }
            }
        }

        public void SetHoldingItem()
        {
            if (itemHolder != null)
            {
                animManager.SetBoolHolding(false);
                Destroy(itemHolder.gameObject);
                itemHolder = null;
            }

            if (CurrentItem != null)
            {
                animManager.SetBoolHolding(true);
                CurrentItem.GetItemPrefab(out GameObject item);
                if (item != null && itemHolder == null)
                    itemHolder = Instantiate(item, itemSpawnPoint);
            }
        }

        private void ShowHighlight(Collider target)
        {
            var highlight = target.GetComponent<HighlightObject>();

            if (highlight != null)
                highlight.ShowHighlight();
        }

        private void HideHighlight(Collider target)
        {
            var highlight = target.GetComponent<HighlightObject>();

            if (highlight != null)
                highlight.HideHighlight();
        }
        #endregion

        #region Interaction Output
        public void GetGameStarted()
        {
            StageManager.instance.GameStart();
        }

        public void GetEndDay()
        {
            StageManager.instance.EndDay();
        }

        public void GetOrder(Kitchen.Table table)
        {
            Debug.LogWarning("table null ? : " + (table == null));
            AudioManager.instance.PlayStageSFXOneShot("GetItem");

            if (CurrentItem != null)
            {
                var match = table.GetItemDataMatchingPlayer(CurrentItem);
                if (match != null && CheckNameItem(CurrentItem, match.itemName))
                {
                    CurrentItemCount++;
                    ShowOrderIcon(match.order.type);
                }
            }
            else
            {
                var order = table.GetItemDataWithLowestHealthTime(null);
                if (order != null)
                {
                    CurrentItem = order;
                    CurrentItemCount++;
                    ShowOrderIcon(order.order.type);
                }
            }
        }

        public void GetIngredient(IngredientBox ingredientBox)
        {
            Debug.LogWarning("Item null ? : " + (ingredientBox == null));

            if (!useTwoHand)
            {
                if (CurrentItem != null && CurrentItem.itemName == ingredientBox.ingredientItem.itemName)
                {
                    CurrentItemCount++;
                    Debug.LogWarning("Get IGD ++");
                }
                else if (CurrentItem == null)
                {
                    Debug.LogWarning("Get IGD New");
                    CurrentItem = ingredientBox.ingredientItem;
                    CurrentItemCount++;
                }
                AudioManager.instance.PlayStageSFXOneShot("GetItem");
                return;
            }
            else
            {
                if (CheckNameItem(CurrentItem, ingredientBox.ingredientItem.itemName))
                {
                    // Logic เดียวกันกับด้านบน แต่รองรับสองมือ + การสลับมือ
                    if (CurrentItem != null && CurrentItem.itemName == ingredientBox.ingredientItem.itemName)
                    {
                        CurrentItemCount++;
                        Debug.LogWarning("Get IGD ++");
                    }
                    else if (CurrentItem == null)
                    {
                        Debug.LogWarning("Get IGD New");
                        CurrentItem = ingredientBox.ingredientItem;
                        CurrentItemCount++;
                    }
                    AudioManager.instance.PlayStageSFXOneShot("GetItem");
                    return;
                }
            }
        }

        public void GetFood(FoodTable foodTable)
        {
            var food = foodTable.GiveFood(CurrentItem);

            if (food == null)
            {
                Debug.LogError("Null");
                return;
            }

            //Debug.Log("What food? : " + food.FoodName);        

            if (!useTwoHand)
            {
                if (CurrentItem == null)
                {
                    foreach (var foodData in StageManager.instance.itemList.FoodItem)
                    {
                        if (foodData.food.FoodName == food.FoodName)
                        {
                            CurrentItem = foodData;
                            CurrentItemCount = 1;
                            return;
                        }
                    }
                }
                else
                {
                    if (CurrentItem.food.FoodName == food.FoodName)
                    {
                        CurrentItemCount++;
                    }
                }
                AudioManager.instance.PlayStageSFXOneShot("GetFood");
                return;
            }
            else
            {
                if (CheckNameItem(CurrentItem, food.FoodName))
                {
                    // Logic เดียวกันกับด้านบน แต่รองรับสองมือ + การสลับมือ
                    if (CurrentItem == null)
                    {
                        foreach (var foodData in StageManager.instance.itemList.FoodItem)
                        {
                            if (foodData.food.FoodName == food.FoodName)
                            {
                                CurrentItem = foodData;
                                CurrentItemCount = 1;
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (CurrentItem.food.FoodName == food.FoodName)
                        {
                            CurrentItemCount++;
                        }
                    }
                    AudioManager.instance.PlayStageSFXOneShot("GetFood");
                    return;
                }
            }
        }

        public void GetTrash(CustomerTrash customerTrash)
        {
            if (!useTwoHand)
            {
                if (CurrentItem != null)
                {
                    CurrentItemCount++;
                    Debug.LogWarning("Get Trash ++");
                }
                else if (CurrentItem == null)
                {
                    Debug.LogWarning("Get Trash New");
                    CurrentItem = StageManager.instance.itemList.trashItem;
                    CurrentItemCount++;
                }

                AudioManager.instance.PlayStageSFXOneShot("GetTrashBag");
                StageManager.instance.currentCustomerDieCount--;
                Destroy(customerTrash.gameObject);
                return;
            }
            else
            {
                if (CanReceiveItem(CurrentItem, ItemType.Trash))
                {
                    // Logic เดียวกันกับด้านบน แต่รองรับสองมือ + การสลับมือ
                    if (CurrentItem != null)
                    {
                        CurrentItemCount++;
                        Debug.LogWarning("Get Trash ++");
                    }
                    else if (CurrentItem == null)
                    {
                        Debug.LogWarning("Get Trash New");
                        CurrentItem = StageManager.instance.itemList.trashItem;
                        CurrentItemCount++;
                    }

                    AudioManager.instance.PlayStageSFXOneShot("GetTrashBag");
                    StageManager.instance.currentCustomerDieCount--;
                    Destroy(customerTrash.gameObject);
                    return;
                }
            }
        }

        public void GetUseItem(ItemDataBase item)
        {
            if (useItemDataBase == null)
            {
                useItemDataBase = item;
            }
        }

        public void GetOpenShop(ShopMerchantManager shopMerchant)
        {
            if (shopMerchant != null)
            {
                shopMerchant.OpenShop();
            }
        }

        public void GiveOrderKT()
        {
            kitchenTable.AddOrders(CurrentItem.order, CurrentItemCount);
            CurrentItemCount = 0;
        }

        public void GiveOrderBT()
        {
            blenderTable.AddOrders(CurrentItem.order, CurrentItemCount);
            CurrentItemCount = 0;
        }

        public void GiveIngredientChef()
        {
            if (chef.hasGetIngredients)
            {
                chef.ReceiveIngredient(CurrentItem.ingredient, 1, out bool isCollect);
                //ลบ Item ออกจาก Player
                if (isCollect)
                {
                    CurrentItemCount--;
                    AudioManager.instance.PlayStageSFXOneShot("GetItem");
                }
            }
        }

        public void GiveIngredientBlender()
        {
            if (blenderMaker.hasGetIngredients)
            {
                blenderMaker.ReceiveIngredient(CurrentItem.ingredient, 1, out bool isCollect);
                //ลบ Item ออกจาก Player
                if (isCollect)
                {
                    CurrentItemCount--;
                    AudioManager.instance.PlayStageSFXOneShot("GetItem");
                }
            }
        }

        public void GiveIngredientToTrashCan()
        {
            if (CurrentItem != null)
            {
                CurrentItemCount--;
            }
        }

        public void GiveIngredientToItemGenerator(UseItemGenerator itemGenerator)
        {
            itemGenerator.GetIngredient(CurrentItem);
        }

        public void GiveIngredientToEnemy(MonsterGiverStatus monsterGiver)
        {
            AudioManager.instance.PlayStageSFXOneShot("GetFood");

            monsterGiver.TakeIngredient(CurrentItem);
        }

        public void GiveFoodToCustomer(Kitchen.Table table)
        {
            AudioManager.instance.PlayStageSFXOneShot("GetFood");

            table.TakeFood(CurrentItem);
        }

        public void GiveFoodToReturnPoint(ReturnPoint returnPoint)
        {
            AudioManager.instance.PlayStageSFXOneShot("GetFood");

            returnPoint.ReturnFood(CurrentItem.food);
        }

        public void GiveTrash()
        {
            if (CurrentItem != null)
            {
                AudioManager.instance.PlayStageSFXOneShot("DropTarshBag");

                CurrentItemCount = 0;
            }
        }

        public void StartCleanUp(CustomerDead customer)
        {
            if (!cleaning)
                StartCoroutine(Cleaning(customer));
        }

        public void StartRepairing(BarricadeStatus barricadeStatus)
        {
            if (!repairing)
                StartCoroutine(Repairing(barricadeStatus));
        }

        public void ActiveTwohand()
        {
            useTwoHand = true;
            isUsingFirstHand = true;
            itemBG.SetActive(false);
        }

        private void UpdateActiveHandUI()
        {
            if (!useTwoHand)
            {
                return;
            }

            if (isUsingFirstHand)
            {
                twoHandAnim.Play("SecondHand");
            }
            else
            {
                twoHandAnim.Play("FirstHand");
            }

            isUsingFirstHand = !isUsingFirstHand;
            SetHoldingItem();
        }

        // เมธอดแยกโชว์ไอคอน เพื่อให้โค้ดหลักอ่านง่ายขึ้น
        private void ShowOrderIcon(OrderType type)
        {
            bool food = type == OrderType.Food;
            if (!useTwoHand)
            {
                if (food)
                {
                    orderIcon.SetActive(true);
                    waterOrderIcon.SetActive(false);
                }
                else
                {
                    waterOrderIcon.SetActive(true);
                    orderIcon.SetActive(false);
                }
            }
            else
            {
                if (isUsingFirstHand)
                {
                    if (food)
                    {
                        orderFirstIcon.SetActive(true);
                        waterOrderFirstIcon.SetActive(false);
                    }
                    else
                    {
                        waterOrderFirstIcon.SetActive(true);
                        orderFirstIcon.SetActive(false);
                    }
                }
                else
                {
                    if (food)
                    {
                        orderSecondIcon.SetActive(true);
                        waterOrderSecondIcon.SetActive(false);
                    }
                    else
                    {
                        waterOrderSecondIcon.SetActive(true);
                        orderSecondIcon.SetActive(false);
                    }
                }
            }
        }
        #endregion
        IEnumerator Cleaning(CustomerDead customer)
        {
            while (inputManager.isInteractAction)
            {
                cleaning = true;
                yield return new WaitForSeconds(.1f);
                if (customer == null)
                {
                    inputManager.ResetAction();
                    inputManager.isInteractAction = false;
                    animManager.CleanUp(inputManager.isInteractAction);
                    break;
                }

                if (inputManager.isInteractAction)
                {
                    customer.CleanUp();
                    AudioManager.instance.PlayStageSFXOneShot("GetItem");
                }
            }

            if (customer != null)
            {
                customer.StopCleanUp();
            }

            cleaning = false;
        }

        IEnumerator Repairing(BarricadeStatus barricadeStatus)
        {
            while (inputManager.isInteractAction)
            {
                repairing = true;
                yield return new WaitForSeconds(.1f);
                if (barricadeStatus == null || !barricadeStatus.canRepair)
                {
                    inputManager.ResetAction();
                    inputManager.isInteractAction = false;
                    animManager.CleanUp(inputManager.isInteractAction);
                    break;
                }

                if (inputManager.isInteractAction)
                {
                    barricadeStatus.Repair();
                    AudioManager.instance.PlayStageSFXOneShot("GetItem");
                }
            }

            if (barricadeStatus != null && barricadeStatus.canRepair)
            {

                barricadeStatus.StopRepair();
            }
            repairing = false;
        }
    }
}