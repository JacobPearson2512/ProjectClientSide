using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int id;
    public string username;
    public int maxHP;
    public int currentHP;
    public float damage;
    public float defense;
    public string currentMove;
    public int numberPotions;
    public bool isBlocking = false;
    public int timesHit = 0;
    public bool hasWon = false;

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
