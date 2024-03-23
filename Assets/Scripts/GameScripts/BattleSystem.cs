using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SnapshotRecording;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public TextMeshProUGUI dialogue;
    public TextMeshProUGUI bagButtonText;
    public BattleState state;

    public GameObject moveSelector;
    public GameObject waitingUI;

    Unit localPlayerUnit;
    Unit enemyUnit;

    GameObject enemyPlayer;
    GameObject localPlayer;

    public BattleUI playerUI;
    public BattleUI enemyUI;

    public int numberPotions = 0;

    Animator playerAnimator;
    Animator enemyAnimator;

    GlobalState globalState;
    public SnapshotManager snapshotManager;

    public int snapshotID = 0;

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
        snapshotManager = new SnapshotManager();

        numberPotions = 3;
        moveSelector.SetActive(false);
        waitingUI.SetActive(false);
        state = BattleState.START;
        StartCoroutine(BeginBattle());
        
    }
    IEnumerator BeginBattle()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerAnimator = localPlayer.GetComponentInChildren<Animator>();
        playerAnimator.SetTrigger("CombatBegin");
        enemyAnimator = enemyPlayer.GetComponentInChildren<Animator>();

        playerUI.SetUI(localPlayerUnit);
        enemyUI.SetUI(enemyUnit);

        dialogue.text = enemyUnit.username + " appeared!";
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
        RecordState();
        snapshotManager.initiatedSnapshot = true;
    }

    public IEnumerator Block() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        playerAnimator.SetTrigger("Block");
        yield return new WaitForSeconds(2f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public IEnumerator Slash() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        playerAnimator.SetTrigger("Slash");
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

    public IEnumerator Whirlwind() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        playerAnimator.SetTrigger("Whirlwind");
        enemyUI.SetHP(enemyUnit.currentHP);
        enemyUI.SetDefense(enemyUnit.defense);
        yield return new WaitForSeconds(1f);
        dialogue.text = "Reduced " + enemyUnit.username + "'s defense by 20%!";
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

    public IEnumerator Flurry() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        int timesHit = localPlayerUnit.timesHit;
        for (int i = 1; i < timesHit + 1; i++)
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
            
        }
        yield return new WaitForSeconds(1f);
        dialogue.text = "Hit " + enemyUnit.username + " " + timesHit + " times!";
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
        waitingUI.SetActive(false);
        yield return new WaitForSeconds(2f);
        dialogue.text = "You used a potion...";
        yield return new WaitForSeconds(1f);
        dialogue.text = $"{localPlayerUnit.username} healed " + 50 + " HP!";
        playerUI.SetHP(localPlayerUnit.currentHP);
        bagButtonText.text = "Heal\n(Potions: " + localPlayerUnit.numberPotions + ")";
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    } // Executed after the server sends return packet

    IEnumerator EnemyTurn() // TODO: Executed after a move's effect is displayed client side. Come back near end of project to maybe change golem to player, add animations etc too.
    {
        enemyAnimator.SetTrigger("Hit");
        if (localPlayerUnit.currentMove == "Protect")
        {
            dialogue.text = enemyUnit.username + " used " + enemyUnit.currentMove;
            yield return new WaitForSeconds(1f);
            dialogue.text = localPlayerUnit.username + " blocked the attack!";

            yield return new WaitForSeconds(1f);
            state = BattleState.PLAYERTURN;
            playerAnimator.SetTrigger("EndOfTurn");
            PlayerTurn();
        }
        else if (enemyUnit.currentMove == "Heal")
        {
            dialogue.text = enemyUnit.username + " used a potion...";
            enemyUI.SetHP(enemyUnit.currentHP);
            yield return new WaitForSeconds(1f);
            dialogue.text = enemyUnit.username + " healed 50HP!";
            yield return new WaitForSeconds(1f);
            state = BattleState.PLAYERTURN;
            playerAnimator.SetTrigger("EndOfTurn");
            PlayerTurn();
        }
        else
        {
            dialogue.text = enemyUnit.username + " used " + enemyUnit.currentMove;
            yield return new WaitForSeconds(1f);
            playerAnimator.SetTrigger("Hit");
            playerUI.SetHP(localPlayerUnit.currentHP);
            playerUI.SetDefense(localPlayerUnit.defense);
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

    #region onClicks
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
            return;
        }
        moveSelector.SetActive(false);
        waitingUI.SetActive(true);
        ClientSend.MoveSelected("Heal");
    }

    public void OnBlock()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Protect");
        waitingUI.SetActive(true);
        dialogue.text = localPlayerUnit.name + " used Block!";
    }

    public void OnSlash()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Slash");
        waitingUI.SetActive(true);
        dialogue.text = localPlayerUnit.username + " used Slash!";
    }

    public void OnWhirlwind()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Whirlwind");
        waitingUI.SetActive(true);
        dialogue.text = localPlayerUnit.username + " used Whirlwind Blade!";
    }

    public void OnFlurry()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        moveSelector.SetActive(false);
        ClientSend.MoveSelected("Flurry");
        waitingUI.SetActive(true);
        dialogue.text = localPlayerUnit.username + " used Flurry!";
    }
    #endregion

    public void RecordState()
    {
        if (!snapshotManager.initiatedSnapshot)
        {
            if (localPlayerUnit.id == 1)
            {
                globalState = new SnapshotRecording.GlobalState(localPlayerUnit.currentHP, enemyUnit.currentHP, localPlayerUnit.defense, enemyUnit.defense, localPlayerUnit.numberPotions, enemyUnit.numberPotions);
            }
            else
            {
                globalState = new SnapshotRecording.GlobalState(enemyUnit.currentHP, localPlayerUnit.currentHP, enemyUnit.defense, localPlayerUnit.defense, enemyUnit.numberPotions, localPlayerUnit.numberPotions);
            }
            Snapshot snapshot = snapshotManager.TakeSnapshot(snapshotID, globalState);
            if (snapshot != null)
            {
                Debug.Log($"Snapshot {snapshot.snapshotId}:\nPlayer 1: <HP: {snapshot.state.player1Health}, Defense: {snapshot.state.player1Defense}, Potions: {snapshot.state.player1Potions}> Player 2: <HP: {snapshot.state.player2Health}, Defense: {snapshot.state.player2Defense}, Potions: {snapshot.state.player2Potions}>");
            }
            snapshotID += 1;
            ClientSend.Marker();
        }
        else
        {
            Debug.Log("Marker Received, snapshot algorithm complete.");
            // record state of Server -> client channel. In this case, always empty.
            snapshotManager.initiatedSnapshot = false;
        }

    }
}
