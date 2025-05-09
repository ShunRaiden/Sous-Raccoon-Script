using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SousRaccoon.Customer.CustomerMovement;

namespace SousRaccoon.Kitchen
{
    public class Table : MonoBehaviour
    {
        public List<Chair> chairs = new List<Chair>(); // รายการเก้าอี้ทั้งหมดของโต๊ะนี้

        public Dictionary<string, ItemDataBase> customerMenuDics = new Dictionary<string, ItemDataBase>();

        // ฟังก์ชันสำหรับตรวจสอบว่ามีเก้าอี้ว่างหรือไม่
        public bool TryGetAvailableChair(out Chair chair)
        {
            // ค้นหาเก้าอี้ที่ยังไม่ถูกใช้งาน
            chair = chairs.FirstOrDefault(c => !c.IsOccupied);
            return chair != null; // คืนค่า true ถ้าเจอเก้าอี้ว่าง, false ถ้าไม่เจอ
        }

        public bool TryGetHasCustomer()
        {
            return chairs.Exists(chair => chair.IsSitting);
        }

        // ฟังก์ชันสำหรับเปลี่ยนสถานะเก้าอี้เป็นถูกใช้งาน
        public void OccupyChair(Chair chair)
        {
            chair.IsOccupied = true; // เปลี่ยนสถานะเก้าอี้เป็นถูกใช้งาน
        }

        // ฟังก์ชันสำหรับเปลี่ยนสถานะเก้าอี้เป็นว่าง
        public void FreeChair(Chair chair)
        {
            chair.IsOccupied = false; // เปลี่ยนสถานะเก้าอี้เป็นว่าง
            chair.IsTakeMenu = false;
            chair.IsGetFood = false;
            chair.customerStatus = null;
        }

        public bool HasAvailableChair()
        {
            return chairs.Any(chair => !chair.IsOccupied); // ตรวจสอบว่ามีเก้าอี้ตัวไหนว่างหรือไม่
        }

        public ItemDataBase GetItemDataWithLowestHealthTime(ItemDataBase playerHolderItem)
        {
            // ถ้า playerHolderItem เป็น null ให้ return Item Order ของเก้าอี้ที่มี currentHealthTime น้อยที่สุด
            if (playerHolderItem == null)
            {
                // หาเก้าอี้ที่มี currentHealthTime น้อยที่สุด และ state เป็น ActionState.Menu
                var chairsWithLowestHealth = chairs
                    .Where(chair => chair.customerStatus != null &&
                                    !chair.IsTakeMenu &&
                                    chair.customerStatus.canGiveOrder &&
                                    chair.CustomerMovement.state == ActionState.Menu)  // เพิ่มเงื่อนไขนี้
                    .OrderBy(chair => chair.customerStatus.currentHealthTime)
                    .ToList();

                // ตรวจสอบว่าเจอ Chair อย่างน้อย 1 ตัว
                if (chairsWithLowestHealth.Count > 0)
                {
                    // เลือกเก้าอี้ตัวแรกที่ยังไม่ถูกตั้งค่า IsTakeMenu
                    var chairWithLowestHealth = chairsWithLowestHealth.FirstOrDefault(chair => !chair.IsTakeMenu);

                    // ตรวจสอบว่ามีเก้าอี้ที่ยังไม่ถูกตั้งค่า
                    if (chairWithLowestHealth != null)
                    {
                        var chairName = chairWithLowestHealth.chairName;

                        // ตรวจสอบว่ามีข้อมูลใน customerMenuDics ที่ตรงกับ Chair นี้
                        if (customerMenuDics.ContainsKey(chairName))
                        {
                            var chairItem = customerMenuDics[chairName];

                            // ตรวจสอบว่า Item นั้นเป็นประเภท Order หรือไม่
                            if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                            {
                                chairWithLowestHealth.IsTakeMenu = true; // ตั้งค่าว่าเก้าอี้นี้ได้ส่ง Order ไปแล้ว
                                return chairItem; // คืนค่า Item Order ของเก้าอี้ที่ currentHealthTime น้อยที่สุด
                            }
                        }
                    }
                }

                Debug.LogWarning("ไม่พบ Item Order ของเก้าอี้ที่มี currentHealthTime น้อยที่สุด");
                return null;
            }

            // ตรวจสอบว่า Player มี Item หรือไม่ และ Item นั้นเป็นประเภท Order หรือเปล่า
            if (playerHolderItem.itemType != ItemDataBase.ItemType.Order)
            {
                Debug.LogWarning("Player ไม่มี Item หรือไม่ใช่ประเภท Order");
                return null;
            }

            // ตรวจสอบว่า Dictionary customerMenuDics ไม่ว่าง
            if (customerMenuDics == null || customerMenuDics.Count == 0)
            {
                Debug.LogWarning("ไม่มีข้อมูลใน customerMenuDics");
                return null;
            }

            // หา Chair ที่มี currentHealthTime น้อยที่สุด ที่ยังไม่ได้ส่งเมนูไปแล้ว และ state เป็น ActionState.Menu
            var chairsWithLowestHealthOrder = chairs
                .Where(chair => chair.customerStatus != null &&
                                !chair.IsTakeMenu &&
                                chair.CustomerMovement.state == ActionState.Menu)  // เพิ่มเงื่อนไขนี้
                .OrderBy(chair => chair.customerStatus.currentHealthTime)
                .ToList();

            // ตรวจสอบว่ามีเก้าอี้ที่ยังไม่ถูกตั้งค่า
            if (chairsWithLowestHealthOrder.Count > 0)
            {
                // เลือกเก้าอี้ตัวแรกที่ยังไม่ถูกตั้งค่า IsTakeMenu
                var chairWithLowestHealthOrder = chairsWithLowestHealthOrder.FirstOrDefault(chair => !chair.IsTakeMenu);

                if (chairWithLowestHealthOrder != null)
                {
                    var chairName = chairWithLowestHealthOrder.chairName;

                    // ตรวจสอบว่ามีข้อมูลใน customerMenuDics ที่ตรงกับ Chair นี้
                    if (customerMenuDics.ContainsKey(chairName))
                    {
                        var chairItem = customerMenuDics[chairName];

                        // ตรวจสอบว่า Item ในโต๊ะซ้ำกับที่ผู้เล่นถือหรือไม่
                        if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                        {
                            if (string.Equals(chairItem.order.FoodType.FoodName, playerHolderItem.order.FoodType.FoodName, StringComparison.Ordinal))
                            {
                                chairWithLowestHealthOrder.IsTakeMenu = true; // ตั้งค่าว่าเก้าอี้นี้ได้ส่ง Order ไปแล้ว
                                return chairItem; // คืนค่า Item ถ้ามีซ้ำ
                            }
                        }
                    }
                }
            }

            // ถ้าไม่พบเก้าอี้ที่ตรงเงื่อนไขหรือไม่มี Item ซ้ำ
            Debug.LogWarning("ไม่พบ Item ซ้ำกับที่ผู้เล่นถือ");
            return null;
        }

        public ItemDataBase GetItemDataMatchingPlayer(ItemDataBase playerHolderItem)
        {
            // ตรวจสอบว่า Player ถือไอเท็มหรือไม่
            if (playerHolderItem == null)
            {
                Debug.LogWarning("Player ไม่มีไอเท็มใน itemDataBase");
                return null;
            }

            // ตรวจสอบว่า playerHolderItem เป็นประเภท Order หรือไม่
            if (playerHolderItem.itemType != ItemDataBase.ItemType.Order)
            {
                Debug.LogWarning("Player ไม่ได้ถือ Order");
                return null;
            }

            // ค้นหาเก้าอี้ที่มีไอเท็มที่ซ้ำกับของ Player โดยเรียงตาม currentHealthTime จากน้อยไปมาก
            var chairsWithMatchingItem = chairs
                .Where(chair => chair.customerStatus != null &&
                                !chair.IsTakeMenu &&
                                chair.customerStatus.canGiveOrder &&
                                chair.CustomerMovement.state == ActionState.Menu &&
                                customerMenuDics.ContainsKey(chair.chairName) && // ตรวจสอบว่าเก้าอี้มี Order ใน customerMenuDics
                                customerMenuDics[chair.chairName].order.FoodType.FoodName == playerHolderItem.order.FoodType.FoodName) // ตรวจสอบว่าไอเท็มในเก้าอี้ซ้ำกับที่ผู้เล่นถือ
                .OrderBy(chair => chair.customerStatus.currentHealthTime)
                .ToList();

            // ตรวจสอบว่ามีเก้าอี้ที่ตรงกับไอเท็มที่ผู้เล่นถือ
            if (chairsWithMatchingItem.Count > 0)
            {
                var chairWithMatchingItem = chairsWithMatchingItem.FirstOrDefault(chair => !chair.IsTakeMenu);

                if (chairWithMatchingItem != null)
                {
                    var chairName = chairWithMatchingItem.chairName;

                    // ตรวจสอบว่ามีข้อมูลใน customerMenuDics ที่ตรงกับ Chair นี้
                    if (customerMenuDics.ContainsKey(chairName))
                    {
                        var chairItem = customerMenuDics[chairName];

                        // ตรวจสอบว่า Item นั้นเป็นประเภท Order หรือไม่
                        if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                        {
                            chairWithMatchingItem.IsTakeMenu = true; // ตั้งค่าว่าเก้าอี้นี้ได้ส่ง Order ไปแล้ว
                            return chairItem; // คืนค่า Item Order ของเก้าอี้ที่ตรงกับที่ผู้เล่นถือ
                        }
                    }
                }
            }

            // ถ้าไม่พบเก้าอี้ที่ตรงเงื่อนไข
            Debug.LogWarning("ไม่พบ Item ที่ซ้ำกับที่ผู้เล่นถือ");
            return null;
        }

        public void TakeFood(ItemDataBase foodPlayer)
        {
            // ตรวจสอบว่า Player ถือ Item ประเภทอาหารหรือไม่
            if (foodPlayer == null || foodPlayer.itemType != ItemDataBase.ItemType.Food)
            {
                Debug.LogWarning("ผู้เล่นไม่ได้ถืออาหารหรือถือ Item ที่ไม่ใช่อาหาร");
                return;
            }

            // ตรวจสอบว่ามีลูกค้าที่รออาหารอยู่หรือไม่
            if (customerMenuDics == null || customerMenuDics.Count == 0)
            {
                Debug.LogWarning("ไม่มีลูกค้าคนใดที่กำลังรออาหาร");
                return;
            }

            // หาเก้าอี้ที่ลูกค้ามี currentHealthTime ต่ำที่สุด ที่ยังไม่ได้รับอาหาร และลูกค้าอยู่ในสถานะรออาหาร (ActionState.Wait)
            var chairWithMatchingOrder = customerMenuDics.Keys
                .Where(chairName =>
                {
                    // ค้นหา Chair จาก chairName
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair != null &&
                           chair.customerStatus != null &&
                           !chair.IsGetFood &&
                           chair.CustomerMovement.state == ActionState.Wait; // เพิ่มเงื่อนไขนี้
                })
                .OrderBy(chairName =>
                {
                    // ค้นหา Chair อีกครั้งเพื่อเข้าถึง currentHealthTime
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair?.customerStatus.currentHealthTime ?? float.MaxValue;
                })
                .FirstOrDefault(chairName =>
                {
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair != null && customerMenuDics[chairName].order.FoodType.FoodName == foodPlayer.food.FoodName;
                });

            Debug.LogWarning("Order : " + chairWithMatchingOrder);

            // ถ้าเจอลูกค้าที่มี currentHealthTime น้อยที่สุดและตรงกับอาหารที่ผู้เล่นถือ
            if (chairWithMatchingOrder != null)
            {
                var chair = chairs.First(c => c.chairName == chairWithMatchingOrder);
                chair.IsGetFood = true; // กำหนดว่าเก้าอี้นี้ได้รับอาหารแล้ว

                chair.SpawnFood(foodPlayer.food.FoodPref);

                // ลบเมนูออกจาก Dictionary เมื่ออาหารถูกส่งไปแล้ว
                customerMenuDics.Remove(chairWithMatchingOrder);
                StageManager.instance.playerKitchenAction.CurrentItemCount--;
                Debug.Log($"อาหารถูกส่งให้กับลูกค้าที่มี currentHealthTime ต่ำที่สุด: {chair.chairName}");
            }
            else
            {
                Debug.LogWarning("ไม่พบลูกค้าที่ต้องการอาหารนี้");
            }
        }
    }
}

