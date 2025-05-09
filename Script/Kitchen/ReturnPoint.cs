using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using System.Linq;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class ReturnPoint : MonoBehaviour
    {
        public ReturnPointType type;

        public enum ReturnPointType
        {
            kitchen,
            blender
        }

        KitchenTable kitchenTable;
        BlenderTable blenderTable;

        private void Start()
        {
            kitchenTable = FindAnyObjectByType<KitchenTable>();
            blenderTable = FindAnyObjectByType<BlenderTable>();
        }

        public bool ReturnFood(FoodSO foodToReturn)
        {
            switch (type)
            {
                case ReturnPointType.kitchen:
                    return OnKitchenTable(foodToReturn);

                case ReturnPointType.blender:
                    return OnBlenderTable(foodToReturn);
                default:
                    // ����������з����ҧ ����ʴ���ͤ�����͹
                    Debug.LogWarning("���������˵�������������");
                    return false; // ��ä׹������������
            }

        }

        public bool OnKitchenTable(FoodSO foodToReturn)
        {
            // ���� FoodTable ���������ê�Դ���ǡѹ
            FoodTable matchingFoodTable;

            matchingFoodTable = kitchenTable.foodTableList.FirstOrDefault(ft => ft.foodData == foodToReturn);

            if (matchingFoodTable != null)
            {
                // ����� FoodTable ��蹷��������ê�Դ���ǡѹ �����ӹǹ���й��
                matchingFoodTable.foodIndex += 1;
                kitchenTable.foodStorage[foodToReturn] += 1;
                matchingFoodTable.SetIndex();
                Debug.Log("���������� FoodTable �������������");

                StageManager.instance.playerKitchenAction.CurrentItemCount--;

                return true; // �׹����������
            }
            else
            {
                // ����з����ҧ (�������������躹���)
                var emptyFoodTable = kitchenTable.foodTableList.FirstOrDefault(ft => ft.foodData == null);

                if (emptyFoodTable != null)
                {
                    // �������з����ҧ ������������й��
                    emptyFoodTable.foodData = foodToReturn;
                    kitchenTable.foodStorage[foodToReturn] = 1; // ������鹴��¨ӹǹ 1

                    // �ѻവ Icon
                    emptyFoodTable.foodIcon.sprite = foodToReturn.FoodIcon;

                    // �ѻവ�ӹǹ� foodTable
                    emptyFoodTable.foodIndex = 1;
                    emptyFoodTable.SetIndex();

                    // ���ҧ foodPref ���躹�����ҧ
                    emptyFoodTable.foodPref = Instantiate(foodToReturn.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;

                    // �ʴ� UI
                    emptyFoodTable.canvas.SetActive(true);

                    StageManager.instance.playerKitchenAction.CurrentItemCount--;

                    Debug.Log("�������������������ҧ");
                    return true; // �׹����������
                }
                else
                {
                    // ����������з����ҧ ����ʴ���ͤ�����͹
                    Debug.LogWarning("�������з����ҧ����Ѻ����÷���ͧ��ä׹");
                    return false; // ��ä׹������������
                }
            }
        }

        public bool OnBlenderTable(FoodSO foodToReturn)
        {
            // ���� FoodTable ���������ê�Դ���ǡѹ
            FoodTable matchingFoodTable;

            matchingFoodTable = blenderTable.foodTableList.FirstOrDefault(ft => ft.foodData == foodToReturn);

            if (matchingFoodTable != null)
            {
                // ����� FoodTable ��蹷��������ê�Դ���ǡѹ �����ӹǹ���й��
                matchingFoodTable.foodIndex += 1;
                blenderTable.foodStorage[foodToReturn] += 1;
                matchingFoodTable.SetIndex();
                Debug.Log("���������� FoodTable �������������");

                StageManager.instance.playerKitchenAction.CurrentItemCount--;

                return true; // �׹����������
            }
            else
            {
                // ����з����ҧ (�������������躹���)
                var emptyFoodTable = blenderTable.foodTableList.FirstOrDefault(ft => ft.foodData == null);

                if (emptyFoodTable != null)
                {
                    // �������з����ҧ ������������й��
                    emptyFoodTable.foodData = foodToReturn;
                    blenderTable.foodStorage[foodToReturn] = 1; // ������鹴��¨ӹǹ 1

                    // �ѻവ Icon
                    emptyFoodTable.foodIcon.sprite = foodToReturn.FoodIcon;

                    // �ѻവ�ӹǹ� foodTable
                    emptyFoodTable.foodIndex = 1;
                    emptyFoodTable.SetIndex();

                    // ���ҧ foodPref ���躹�����ҧ
                    emptyFoodTable.foodPref = Instantiate(foodToReturn.FoodPref, emptyFoodTable.spawnPos.position, Quaternion.identity, emptyFoodTable.transform);
                    emptyFoodTable.SetFoodHighlightMaterial();
                    emptyFoodTable.foodCollider.enabled = true;

                    // �ʴ� UI
                    emptyFoodTable.canvas.SetActive(true);

                    StageManager.instance.playerKitchenAction.CurrentItemCount--;

                    Debug.Log("�������������������ҧ");
                    return true; // �׹����������
                }
                else
                {
                    // ����������з����ҧ ����ʴ���ͤ�����͹
                    Debug.LogWarning("�������з����ҧ����Ѻ����÷���ͧ��ä׹");
                    return false; // ��ä׹������������
                }
            }
        }
    }
}