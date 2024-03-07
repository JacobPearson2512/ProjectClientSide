using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int id;
    //[SerializeField]
    public string username;
    [SerializeField]
    public int maxHP;
    [SerializeField]
    public int currentHP;
    [SerializeField]
    public float damage;
    [SerializeField]
    public float defense;
    [SerializeField]
    public string attackName;
    public bool isBlocking = false;

    public bool ReduceHP(float damage)
    {
        damage = damage + (damage * (1.0f - defense));
        int roundedDamage = (int) Mathf.Round(damage);
        currentHP -= roundedDamage;
        if(currentHP <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void usePotion(int potionSize)
    {
        currentHP += potionSize;
    }

    public void Block() 
    {
        isBlocking = true;
    }

    public void unBlock()
    {
        isBlocking = false;
    }
}
