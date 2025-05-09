using SousRaccoon.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class BarricadeStatus : MonoBehaviour
{
    [Header("HP")]
    public int maxHealthPoint;
    public int currentHealthPoint;

    public bool canRepair;
    public bool canTakeDamage;

    [Header("Time")]
    public float maxRepairTime;
    public float currentRepairTime;
    public float countPerTimes = 0.1f; // damage dealt per second

    [Header("VFX")]
    public List<GameObject> repairVFX;

    [Header("UI")]
    public GameObject barricadePanel;
    public Image repairBar;

    public List<GameObject> barricadePref; // index[0] = 100%, index[1] = 55%, index[2] = 20%, index[3] = 0%

    public void SetStat(int maxHP, float repairTime)
    {
        maxHealthPoint = maxHP;
        currentHealthPoint = maxHP;
        maxRepairTime = repairTime;

        canRepair = false;
        canTakeDamage = true;
        barricadePanel.SetActive(false);

        UpdateBarricadeState();

        StopRepair();
    }

    public void TakeDamage(int damage)
    {
        if (!canTakeDamage || damage <= 0) return;

        currentHealthPoint -= damage;
        currentHealthPoint = Mathf.Max(currentHealthPoint, 0); // ป้องกันค่าติดลบ
        AudioManager.instance.PlayStageSFXOneShot("Barricade Take Damage");
        UpdateBarricadeState();
    }

    public void Repair()
    {
        if (!canRepair || repairBar == null) return;

        foreach (var pair in repairVFX)
        {
            pair.gameObject.SetActive(true);
        }

        currentRepairTime += countPerTimes;
        currentRepairTime = Mathf.Min(currentRepairTime, maxRepairTime); // ป้องกันค่าเกิน

        repairBar.fillAmount = currentRepairTime / maxRepairTime;

        if (currentRepairTime >= maxRepairTime)
        {
            FinishRepair();
        }
    }

    public void StopRepair()
    {
        foreach (var pair in repairVFX)
        {
            pair.gameObject.SetActive(false);
        }
    }

    public void FinishRepair()
    {
        canRepair = false;
        canTakeDamage = true;

        currentRepairTime = 0;
        repairBar.fillAmount = currentRepairTime / maxRepairTime;

        currentHealthPoint = maxHealthPoint;

        UpdateBarricadeState();

        foreach (var pair in repairVFX)
        {
            pair.GetComponent<VisualEffect>().SendEvent("FinishBubble");
        }

        barricadePanel.SetActive(false);
        StartCoroutine(ResetPaticle());
    }

    private void UpdateBarricadeState()
    {
        float healthPercentage = ((float)currentHealthPoint / maxHealthPoint) * 100;

        CloseAll();

        if (healthPercentage > 55)
        {
            ActivateBarricade(0); // 100%
        }
        else if (healthPercentage > 20)
        {
            ActivateBarricade(1); // 55%
        }
        else if (healthPercentage > 0)
        {
            ActivateBarricade(2); // 20%
        }
        else
        {
            ActivateBarricade(3); // 0%
            canRepair = true;
            canTakeDamage = false;
            barricadePanel.SetActive(true);
            AudioManager.instance.PlayStageSFXOneShot("Barricade Brake");
        }
    }

    private void CloseAll()
    {
        foreach (var barricade in barricadePref)
        {
            if (barricade != null) barricade.SetActive(false);
        }
    }

    private void ActivateBarricade(int index)
    {
        if (index >= 0 && index < barricadePref.Count && barricadePref[index] != null)
        {
            barricadePref[index].SetActive(true);
        }
    }
    IEnumerator ResetPaticle()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var pair in repairVFX)
        {
            pair.SetActive(false);
        }
    }
}
