using SousRaccoon.Data.Item;
using SousRaccoon.Manager;
using SousRaccoon.Monster;
using SousRaccoon.Player;
using UnityEngine;
using UnityEngine.UI;

public class MonsterGiverStatus : MonsterStatus
{
    [SerializeField] private MonsterGiverMovement movement;

    public PlayerKitchenAction player;

    public ItemDataBase itemDataBase;
    public GameObject igdCurrent;
    public Transform igdSpawnPoint;
    public GameObject requestPanel;
    public Image waittingTimeImage;
    public Image iGDSprite;

    public int timesOfRequest;
    public int currentTimesRequest;

    public bool hasTakeDamage;
    public bool canTakeIGD;

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<MonsterGiverMovement>();
        player = StageManager.instance.playerKitchenAction;
        requestPanel.SetActive(false);
        hasTakeDamage = false;
    }

    public override void TakeDamage(int damage)
    {
        if (hasTakeDamage)
            base.TakeDamage(damage);
    }

    public void GenerateIngredientRequest()
    {
        canTakeIGD = true;
        itemDataBase = StageManager.instance.GenerateIngredient();

        SetUpCanvas(itemDataBase.ingredient.IngredientIcon);
    }

    public void TakeIngredient(ItemDataBase itemGiver)
    {
        if (itemGiver == null || !canTakeIGD)
            return;

        if (itemDataBase.itemName == itemGiver.itemName)
        {
            canTakeIGD = false;
            player.CurrentItemCount--;

            if (currentTimesRequest >= timesOfRequest)
            {
                GenerateIngredientRequest();
            }
            else
            {
                if (igdCurrent == null)
                {
                    igdCurrent = Instantiate(itemGiver.ingredient.IngredientPref, igdSpawnPoint);
                }

                requestPanel.SetActive(false);
                hasTakeDamage = true;
                movement.StartEatting();
            }
        }
    }

    public override void Die()
    {
        base.Die();
        movement.StartDie();
    }

    private void SetUpCanvas(Sprite sprite)
    {
        requestPanel.SetActive(true);
        iGDSprite.sprite = sprite;
    }
}
