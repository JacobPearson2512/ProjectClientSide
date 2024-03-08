using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public TextMeshProUGUI dialogue;
    public TextMeshProUGUI bagButtonText;
    public BattleState state;

    public GameObject moveSelector;

    Unit localPlayerUnit;
    Unit enemyUnit;

    GameObject enemyPlayer;
    GameObject localPlayer;

    public BattleUI playerUI;
    public BattleUI enemyUI;

    public int numberPotions = 0;

    Animator playerAnimator;
    Animator enemyAnimator;

    public void StartBattleSystem()
    {
        localPlayerUnit = GameManager.players[Client.instance.myID];
        Debug.Log($"Plyaer name: {localPlayerUnit.username}");
        localPlayer = localPlayerUnit.gameObject;
        int id2;
        if (Client.instance.myID == 1)
        {
            id2 = 2;
        }
        else
        {
            id2 = 1;
        }
        enemyUnit = GameManager.players[id2]; // TODO figure out what this client id would be
        enemyPlayer = enemyUnit.gameObject;


        numberPotions = 3;
        moveSelector.SetActive(false);
        state = BattleState.START;
        StartCoroutine(BeginBattle());
        
    }
    // TODO: COMMENTS ARE OLD STUFF REMOVED WHEN ADAPTING
    IEnumerator BeginBattle()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerAnimator = localPlayer.GetComponentInChildren<Animator>();
        playerAnimator.SetTrigger("CombatBegin");
        enemyAnimator = enemyPlayer.GetComponentInChildren<Animator>();

        playerUI.SetUI(localPlayerUnit);
        enemyUI.SetUI(enemyUnit);

        dialogue.text = enemyUnit.name + " appeared!";
        bagButtonText.text = "Heal\n(Potions: " + numberPotions + ")";

        yield return new WaitForSeconds(2f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator EndBattle()
    {
        if(state == BattleState.WON)
        {
            enemyAnimator.SetTrigger("Die");
            yield return new WaitForSeconds(1f);
            Destroy(enemyPlayer);
            dialogue.text = "You defeated " + enemyUnit.username;
            yield return new WaitForSeconds(2f);
        }
        else if (state == BattleState.LOST)
        {
            dialogue.text = "You were defeated by " + enemyUnit.username;
            yield return new WaitForSeconds(2f);
            dialogue.text = "Restarting the game...";
            yield return new WaitForSeconds(2f);
        }
    }

    void PlayerTurn()
    {
        dialogue.text = "What will you do?";
    }

    public IEnumerator Block()
    {
        /*moveSelector.SetActive(false);
        playerAnimator.SetTrigger("Block");
        localPlayerUnit.Block();
        
        yield return new WaitForSeconds(2f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());*/
        yield return new WaitForSeconds(2f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public IEnumerator Slash()
    {
        /*moveSelector.SetActive(false);
        playerAnimator.SetTrigger("Slash");
        bool isDead = enemyUnit.ReduceHP(localPlayerUnit.damage);
        enemyUI.SetHP(enemyUnit.currentHP);
        dialogue.text = localPlayerUnit.name + " used Slash!";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }*/
        
        enemyUI.SetHP(enemyUnit.currentHP);
        yield return new WaitForSeconds(2f);
        if (enemyUnit.currentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            if (enemyUnit.currentMove == "Protect")
            {
                dialogue.text = enemyUnit.username + " blocked it!";
            }
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }

    }

    public IEnumerator Whirlwind()
    {
        /*moveSelector.SetActive(false);
        playerAnimator.SetTrigger("Whirlwind");
        bool isDead = enemyUnit.ReduceHP(15);
        enemyUnit.defense = Mathf.Round(enemyUnit.defense * 8f ) / 10;
        enemyUI.SetHP(enemyUnit.currentHP);
        enemyUI.SetDefense(enemyUnit.defense);
        dialogue.text = localPlayerUnit.name + " used Whirlwind Blade!";
        yield return new WaitForSeconds(1f);
        dialogue.text = "Reduced " + enemyUnit.name + "'s defense by 10%!";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }*/
        enemyUI.SetHP(enemyUnit.currentHP);
        enemyUI.SetDefense(enemyUnit.defense);
        yield return new WaitForSeconds(1f);
        dialogue.text = "Reduced " + enemyUnit.username + "2's defense by 10%!";
        yield return new WaitForSeconds(2f);
        if (enemyUnit.currentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            if (enemyUnit.currentMove == "Protect")
            {
                dialogue.text = enemyUnit.username + " blocked it!";
            }
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    public IEnumerator Flurry() // TODO done temp anim cause cant be arsed
    {
        /*moveSelector.SetActive(false);
        int timesHit = Random.Range(2, 6);
        
        for (int i = 1; i < timesHit+1; i++)
        {
            yield return new WaitForSeconds(0.5f);
            if (i % 2 == 0)
            {
                Debug.Log("left");
                playerAnimator.SetTrigger("FlurryLeft");
            }
            else
            {
                Debug.Log("right");
                playerAnimator.SetTrigger("FlurryRight");
            }
            bool isDead = enemyUnit.ReduceHP(15);
            enemyUI.SetHP(enemyUnit.currentHP);
            if (isDead)
            {
                yield return new WaitForSeconds(1f);
                dialogue.text = "Hit " + enemyUnit.name + " " + i + " times!";
                yield return new WaitForSeconds(2f);
                state = BattleState.WON;
                StartCoroutine(EndBattle());
                break;
            }
        }
        if (state != BattleState.WON)
        {
            dialogue.text = "Hit " + enemyUnit.name + " " + timesHit + " times!";
            playerAnimator.SetTrigger("EndOfTurn");
            yield return new WaitForSeconds(2f);
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }*/
        yield return new WaitForSeconds(2f);
        enemyUI.SetHP(enemyUnit.currentHP);
        if (enemyUnit.currentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            if (enemyUnit.currentMove == "Protect")
            {
                dialogue.text = enemyUnit.username + " blocked it!";
            }
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
        
    }

    public IEnumerator PlayerBag()
    {
        /*
        localPlayerUnit.usePotion(50);
        numberPotions -= 1;
        
        yield return new WaitForSeconds(1f);
        
        
        
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());*/
        yield return new WaitForSeconds(2f);
        dialogue.text = "You used a potion...";
        yield return new WaitForSeconds(1f);
        dialogue.text = $"{localPlayerUnit.username} healed " + 50 + " HP!";
        playerUI.SetHP(localPlayerUnit.currentHP);
        bagButtonText.text = "Heal\n(Potions: " + localPlayerUnit.numberPotions + ")";
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn() // add dialogue for potion usage etc.
    {
        dialogue.text = enemyUnit.username + " used " + enemyUnit.currentMove;
        yield return new WaitForSeconds(1f);
        enemyAnimator.SetTrigger("Hit");
        if (localPlayerUnit.currentMove == "Protect")
        {
            dialogue.text = localPlayerUnit.name + " blocked the attack!";
            //localPlayerUnit.unBlock();
            yield return new WaitForSeconds(1f);
            state = BattleState.PLAYERTURN;
            playerAnimator.SetTrigger("EndOfTurn");
            PlayerTurn();
        }
        else
        {
            playerAnimator.SetTrigger("Hit");
            //bool isDead = localPlayerUnit.ReduceHP(enemyUnit.damage);
            playerUI.SetHP(localPlayerUnit.currentHP);
            yield return new WaitForSeconds(1f);
            if (localPlayerUnit.currentHP <= 0)
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
            }
            else
            {
                state = BattleState.PLAYERTURN;
                playerAnimator.SetTrigger("EndOfTurn");
                PlayerTurn();
            }
        }

    }

    public void OnFightButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(true);

    }

    public void OnBagButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        if (localPlayerUnit.numberPotions <= 0)
        {
            //yield break;
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Heal");
        //StartCoroutine(PlayerBag());
    }

    public void OnBlock()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Protect");
        dialogue.text = localPlayerUnit.name + " used Block!";
        //StartCoroutine(Block());
    }

    public void OnSlash()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Slash");
        dialogue.text = localPlayerUnit.username + " used Slash!";
        //StartCoroutine(Slash());
    }

    public void OnWhirlwind()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Whirlwind");
        dialogue.text = localPlayerUnit.username + " used Whirlwind Blade!";
        //StartCoroutine(Whirlwind());
    }

    public void OnFlurry()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Flurry");
        dialogue.text = localPlayerUnit.username + " used Flurry!";
        //StartCoroutine(Flurry());
    }
}
