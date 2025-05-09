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
        public List<Chair> chairs = new List<Chair>(); // ��¡��������������ͧ��й��

        public Dictionary<string, ItemDataBase> customerMenuDics = new Dictionary<string, ItemDataBase>();

        // �ѧ��ѹ����Ѻ��Ǩ�ͺ�������������ҧ�������
        public bool TryGetAvailableChair(out Chair chair)
        {
            // �������������ѧ���١��ҹ
            chair = chairs.FirstOrDefault(c => !c.IsOccupied);
            return chair != null; // �׹��� true �������������ҧ, false ��������
        }

        public bool TryGetHasCustomer()
        {
            return chairs.Exists(chair => chair.IsSitting);
        }

        // �ѧ��ѹ����Ѻ����¹ʶҹ��������繶١��ҹ
        public void OccupyChair(Chair chair)
        {
            chair.IsOccupied = true; // ����¹ʶҹ��������繶١��ҹ
        }

        // �ѧ��ѹ����Ѻ����¹ʶҹ�����������ҧ
        public void FreeChair(Chair chair)
        {
            chair.IsOccupied = false; // ����¹ʶҹ�����������ҧ
            chair.IsTakeMenu = false;
            chair.IsGetFood = false;
            chair.customerStatus = null;
        }

        public bool HasAvailableChair()
        {
            return chairs.Any(chair => !chair.IsOccupied); // ��Ǩ�ͺ��������������˹��ҧ�������
        }

        public ItemDataBase GetItemDataWithLowestHealthTime(ItemDataBase playerHolderItem)
        {
            // ��� playerHolderItem �� null ��� return Item Order �ͧ���������� currentHealthTime ���·���ش
            if (playerHolderItem == null)
            {
                // ������������ currentHealthTime ���·���ش ��� state �� ActionState.Menu
                var chairsWithLowestHealth = chairs
                    .Where(chair => chair.customerStatus != null &&
                                    !chair.IsTakeMenu &&
                                    chair.customerStatus.canGiveOrder &&
                                    chair.CustomerMovement.state == ActionState.Menu)  // �������͹䢹��
                    .OrderBy(chair => chair.customerStatus.currentHealthTime)
                    .ToList();

                // ��Ǩ�ͺ����� Chair ���ҧ���� 1 ���
                if (chairsWithLowestHealth.Count > 0)
                {
                    // ���͡���������á����ѧ���١��駤�� IsTakeMenu
                    var chairWithLowestHealth = chairsWithLowestHealth.FirstOrDefault(chair => !chair.IsTakeMenu);

                    // ��Ǩ�ͺ��������������ѧ���١��駤��
                    if (chairWithLowestHealth != null)
                    {
                        var chairName = chairWithLowestHealth.chairName;

                        // ��Ǩ�ͺ����բ������ customerMenuDics ���ç�Ѻ Chair ���
                        if (customerMenuDics.ContainsKey(chairName))
                        {
                            var chairItem = customerMenuDics[chairName];

                            // ��Ǩ�ͺ��� Item ����繻����� Order �������
                            if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                            {
                                chairWithLowestHealth.IsTakeMenu = true; // ��駤����������������� Order �����
                                return chairItem; // �׹��� Item Order �ͧ�������� currentHealthTime ���·���ش
                            }
                        }
                    }
                }

                Debug.LogWarning("��辺 Item Order �ͧ���������� currentHealthTime ���·���ش");
                return null;
            }

            // ��Ǩ�ͺ��� Player �� Item ������� ��� Item ����繻����� Order ��������
            if (playerHolderItem.itemType != ItemDataBase.ItemType.Order)
            {
                Debug.LogWarning("Player ����� Item �������������� Order");
                return null;
            }

            // ��Ǩ�ͺ��� Dictionary customerMenuDics �����ҧ
            if (customerMenuDics == null || customerMenuDics.Count == 0)
            {
                Debug.LogWarning("����բ������ customerMenuDics");
                return null;
            }

            // �� Chair ����� currentHealthTime ���·���ش ����ѧ���������������� ��� state �� ActionState.Menu
            var chairsWithLowestHealthOrder = chairs
                .Where(chair => chair.customerStatus != null &&
                                !chair.IsTakeMenu &&
                                chair.CustomerMovement.state == ActionState.Menu)  // �������͹䢹��
                .OrderBy(chair => chair.customerStatus.currentHealthTime)
                .ToList();

            // ��Ǩ�ͺ��������������ѧ���١��駤��
            if (chairsWithLowestHealthOrder.Count > 0)
            {
                // ���͡���������á����ѧ���١��駤�� IsTakeMenu
                var chairWithLowestHealthOrder = chairsWithLowestHealthOrder.FirstOrDefault(chair => !chair.IsTakeMenu);

                if (chairWithLowestHealthOrder != null)
                {
                    var chairName = chairWithLowestHealthOrder.chairName;

                    // ��Ǩ�ͺ����բ������ customerMenuDics ���ç�Ѻ Chair ���
                    if (customerMenuDics.ContainsKey(chairName))
                    {
                        var chairItem = customerMenuDics[chairName];

                        // ��Ǩ�ͺ��� Item ���Ы�ӡѺ�������蹶���������
                        if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                        {
                            if (string.Equals(chairItem.order.FoodType.FoodName, playerHolderItem.order.FoodType.FoodName, StringComparison.Ordinal))
                            {
                                chairWithLowestHealthOrder.IsTakeMenu = true; // ��駤����������������� Order �����
                                return chairItem; // �׹��� Item ����ի��
                            }
                        }
                    }
                }
            }

            // �����辺��������ç���͹���������� Item ���
            Debug.LogWarning("��辺 Item ��ӡѺ�������蹶��");
            return null;
        }

        public ItemDataBase GetItemDataMatchingPlayer(ItemDataBase playerHolderItem)
        {
            // ��Ǩ�ͺ��� Player ���������������
            if (playerHolderItem == null)
            {
                Debug.LogWarning("Player ����������� itemDataBase");
                return null;
            }

            // ��Ǩ�ͺ��� playerHolderItem �繻����� Order �������
            if (playerHolderItem.itemType != ItemDataBase.ItemType.Order)
            {
                Debug.LogWarning("Player ������� Order");
                return null;
            }

            // �����������������������ӡѺ�ͧ Player �����§��� currentHealthTime �ҡ������ҡ
            var chairsWithMatchingItem = chairs
                .Where(chair => chair.customerStatus != null &&
                                !chair.IsTakeMenu &&
                                chair.customerStatus.canGiveOrder &&
                                chair.CustomerMovement.state == ActionState.Menu &&
                                customerMenuDics.ContainsKey(chair.chairName) && // ��Ǩ�ͺ����������� Order � customerMenuDics
                                customerMenuDics[chair.chairName].order.FoodType.FoodName == playerHolderItem.order.FoodType.FoodName) // ��Ǩ�ͺ����������������ӡѺ�������蹶��
                .OrderBy(chair => chair.customerStatus.currentHealthTime)
                .ToList();

            // ��Ǩ�ͺ�������������ç�Ѻ������������蹶��
            if (chairsWithMatchingItem.Count > 0)
            {
                var chairWithMatchingItem = chairsWithMatchingItem.FirstOrDefault(chair => !chair.IsTakeMenu);

                if (chairWithMatchingItem != null)
                {
                    var chairName = chairWithMatchingItem.chairName;

                    // ��Ǩ�ͺ����բ������ customerMenuDics ���ç�Ѻ Chair ���
                    if (customerMenuDics.ContainsKey(chairName))
                    {
                        var chairItem = customerMenuDics[chairName];

                        // ��Ǩ�ͺ��� Item ����繻����� Order �������
                        if (chairItem != null && chairItem.itemType == ItemDataBase.ItemType.Order)
                        {
                            chairWithMatchingItem.IsTakeMenu = true; // ��駤����������������� Order �����
                            return chairItem; // �׹��� Item Order �ͧ��������ç�Ѻ�������蹶��
                        }
                    }
                }
            }

            // �����辺��������ç���͹�
            Debug.LogWarning("��辺 Item ����ӡѺ�������蹶��");
            return null;
        }

        public void TakeFood(ItemDataBase foodPlayer)
        {
            // ��Ǩ�ͺ��� Player ��� Item ������������������
            if (foodPlayer == null || foodPlayer.itemType != ItemDataBase.ItemType.Food)
            {
                Debug.LogWarning("���������������������Ͷ�� Item �������������");
                return;
            }

            // ��Ǩ�ͺ������١��ҷ��������������������
            if (customerMenuDics == null || customerMenuDics.Count == 0)
            {
                Debug.LogWarning("������١��Ҥ�㴷����ѧ�������");
                return;
            }

            // �����������١����� currentHealthTime ��ӷ���ش ����ѧ������Ѻ����� ����١��������ʶҹ�������� (ActionState.Wait)
            var chairWithMatchingOrder = customerMenuDics.Keys
                .Where(chairName =>
                {
                    // ���� Chair �ҡ chairName
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair != null &&
                           chair.customerStatus != null &&
                           !chair.IsGetFood &&
                           chair.CustomerMovement.state == ActionState.Wait; // �������͹䢹��
                })
                .OrderBy(chairName =>
                {
                    // ���� Chair �ա����������Ҷ֧ currentHealthTime
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair?.customerStatus.currentHealthTime ?? float.MaxValue;
                })
                .FirstOrDefault(chairName =>
                {
                    var chair = chairs.FirstOrDefault(c => c.chairName == chairName);
                    return chair != null && customerMenuDics[chairName].order.FoodType.FoodName == foodPlayer.food.FoodName;
                });

            Debug.LogWarning("Order : " + chairWithMatchingOrder);

            // ������١��ҷ���� currentHealthTime ���·���ش��еç�Ѻ����÷������蹶��
            if (chairWithMatchingOrder != null)
            {
                var chair = chairs.First(c => c.chairName == chairWithMatchingOrder);
                chair.IsGetFood = true; // ��˹���������������Ѻ���������

                chair.SpawnFood(foodPlayer.food.FoodPref);

                // ź�����͡�ҡ Dictionary ���������ö١�������
                customerMenuDics.Remove(chairWithMatchingOrder);
                StageManager.instance.playerKitchenAction.CurrentItemCount--;
                Debug.Log($"����ö١�����Ѻ�١��ҷ���� currentHealthTime ��ӷ���ش: {chair.chairName}");
            }
            else
            {
                Debug.LogWarning("��辺�١��ҷ���ͧ�������ù��");
            }
        }
    }
}

