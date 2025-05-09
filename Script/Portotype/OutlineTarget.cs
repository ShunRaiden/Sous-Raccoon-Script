using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [Header("Start")]
    public Outline chefOutline;

    [Header("Order")]
    public Outline orderOutline;

    [Header("Trash")]
    public Outline trashOutline;

    [Header("Ingredient")]
    public Outline[] ingredientOutline;

    [Header("Food")]
    public Outline foodOutline;
    public Outline foodReturnOutline;

    public CurrentOutlineType currentOutlineType;
    public enum CurrentOutlineType
    {
        None,
        order,
        trash,
        ingredient,
        food,
    }

    public void SetNoneOutline()
    {
        ResetAllOutline();
        currentOutlineType = CurrentOutlineType.None;
    }

    public void SetOrderOutline()
    {
        ResetAllOutline();
        orderOutline.enabled = true;
        currentOutlineType = CurrentOutlineType.order;
    }

    public void SetTrashOutline()
    {
        ResetAllOutline();
        trashOutline.enabled = true;
        currentOutlineType = CurrentOutlineType.trash;
    }

    public void SetIngredientOutline()
    {
        ResetAllOutline();
        for (int i = 0; i < ingredientOutline.Length; i++)
        {
            ingredientOutline[i].enabled = true;
        }
        currentOutlineType = CurrentOutlineType.ingredient;
    }

    public void SetFoodOutline()
    {
        ResetAllOutline();

        foodOutline.enabled = true;
        foodReturnOutline.enabled = true;

        currentOutlineType = CurrentOutlineType.food;
    }

    public void ResetAllOutline()
    {
        switch (currentOutlineType)
        {
            case CurrentOutlineType.order:
                orderOutline.enabled = false;
                break;
            case CurrentOutlineType.trash:
                trashOutline.enabled = false;
                break;
            case CurrentOutlineType.ingredient:
                for (int i = 0; i < ingredientOutline.Length; i++)
                {
                    ingredientOutline[i].enabled = false;
                }
                break;
            case CurrentOutlineType.food:
                foodOutline.enabled = false;
                foodReturnOutline.enabled = false;
                break;
        }
    }
}
