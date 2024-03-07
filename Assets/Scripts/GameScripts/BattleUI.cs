using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public TextMeshProUGUI nameUI;
    public TextMeshProUGUI debuffUI;
    public Slider hpSliderUI;

    public void SetUI(Unit unit)
    {
        nameUI.text = unit.username;
        hpSliderUI.value = unit.currentHP;
        hpSliderUI.maxValue = unit.maxHP;
        debuffUI.text = ("Defense: " + unit.defense.ToString());
    }

    public void SetHP(int hp)
    {
        hpSliderUI.value = hp;
    }

    public void SetDefense(float defense)
    {
        debuffUI.text = "Defense: " + defense.ToString();
    }
}
