using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SnapshotRecording;
using JetBrains.Annotations;
using System;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, TIE }

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
    public int preTurnEnemyHealth;
    public int preTurnLocalHealth;
    public float preTurnEnemyDef;
    public float preTurnLocalDef;
    public bool recordThisTurn = false;

    Animator playerAnimator;
    Animator enemyAnimator;

    GlobalState globalState;


    public SnapshotManager snapshotManager;

    public int snapshotID = 0;

    public void StartBattleSystem()
    {
        localPlayerUnit = GameManager.players[Client.instance.myID];
        Debug.Log($"Player name: {localPlayerUnit.username}");
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
        preTurnEnemyHealth = enemyUnit.currentHP;
        preTurnLocalHealth = localPlayerUnit.currentHP;
        preTurnEnemyDef = enemyUnit.defense;
        preTurnLocalDef = localPlayerUnit.defense;
        enemyPlayer = enemyUnit.gameObject;
        snapshotManager = new SnapshotManager();
        if (localPlayerUnit.id == 1)
        {
            globalState = new SnapshotRecording.GlobalState(localPlayerUnit.currentHP, enemyUnit.currentHP, localPlayerUnit.defense, enemyUnit.defense, localPlayerUnit.numberPotions, enemyUnit.numberPotions);
        }
        else
        {
            globalState = new SnapshotRecording.GlobalState(enemyUnit.currentHP, localPlayerUnit.currentHP, enemyUnit.defense, localPlayerUnit.defense, enemyUnit.numberPotions, localPlayerUnit.numberPotions);
        }
        Snapshot snapshot = snapshotManager.TakeSnapshot(snapshotID, globalState);
        snapshotID += 1;
        ClientSend.SendInitialState(globalState);
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
        GameManager.instance.ReadMoveHistory();
        if (localPlayerUnit.id == 1)
        {
            globalState = new SnapshotRecording.GlobalState(localPlayerUnit.currentHP, enemyUnit.currentHP, localPlayerUnit.defense, enemyUnit.defense, localPlayerUnit.numberPotions, enemyUnit.numberPotions);
        }
        else
        {
            globalState = new SnapshotRecording.GlobalState(enemyUnit.currentHP, localPlayerUnit.currentHP, enemyUnit.defense, localPlayerUnit.defense, enemyUnit.numberPotions, localPlayerUnit.numberPotions);
        }
        ClientSend.SendFinalState(globalState);
        ClientSend.SendMoveHistory(GameManager.instance.moveHistory);
        if (state == BattleState.WON)
        {
            enemyAnimator.SetTrigger("Die");
            yield return new WaitForSeconds(1f);
            Destroy(enemyPlayer);
            dialogue.text = "You defeated " + enemyUnit.username;
            yield return new WaitForSeconds(2f);
            ClientSend.SendWinner(localPlayerUnit.id);
        }
        else if (state == BattleState.LOST)
        {
            dialogue.text = "You were defeated by " + enemyUnit.username;
            yield return new WaitForSeconds(2f);
            ClientSend.SendWinner(enemyUnit.id);
        }
        else if (state == BattleState.TIE)
        {
            dialogue.text = "You both dealt a fatal blow. The battle is tied.";
            yield return new WaitForSeconds(2f);
            ClientSend.SendWinner(0);
        }
        dialogue.text = "Calculating Inconsistency...";
        yield return new WaitForSeconds(2f);
        

    }

    public void PlayerTurn()
    {
        preTurnEnemyHealth = enemyUnit.currentHP;
        Debug.Log($"PreTurn Health for enemy player: {preTurnEnemyHealth}");
        preTurnLocalHealth = localPlayerUnit.currentHP;
        Debug.Log($"PreTurn Health for local player: {preTurnLocalHealth}");
        preTurnLocalDef = localPlayerUnit.defense;
        preTurnEnemyDef = enemyUnit.defense;
        dialogue.text = "What will you do?";
        //RecordState();
        //snapshotManager.initiatedSnapshot = true;
    }

    public IEnumerator Block() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        playerAnimator.SetTrigger("Block");
        yield return new WaitForSeconds(2f);
        GameManager.instance.AddToMoveHistory(localPlayerUnit.id, "Protect", "Blocked attack.");
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public IEnumerator Slash() // Executed after the server sends return packet
    {
        waitingUI.SetActive(false);
        playerAnimator.SetTrigger("Slash");
        enemyUI.SetHP(enemyUnit.currentHP);
        yield return new WaitForSeconds(2f);
        int dealtDamage = preTurnEnemyHealth - enemyUnit.currentHP;
        if (dealtDamage != (int)Math.Round(20.0f + (20.0f * (1.0f - enemyUnit.defense)))){ // BE AWARE THAT THIS IS BEFORE MOVE HISTORY. MAYBE CALCULATE THEN CHANGE MOVE IN HISTORY? OR KEEP SAME FOR INCONSISTENCY VALUE.
            Debug.Log($"Slash detected as being altered (dealt {dealtDamage}), initiate snapshot algorithm.");
            //RecordState();
            recordThisTurn = true;
            //snapshotManager.initiatedSnapshot = true;
        }
        GameManager.instance.AddToMoveHistory(localPlayerUnit.id, "Slash", "Dealt " + dealtDamage);
        if ((localPlayerUnit.currentHP <= 0 && enemyUnit.currentHP <= 0) || (localPlayerUnit.hasWon && enemyUnit.hasWon))
        {
            state = BattleState.TIE;
            StartCoroutine(EnemyTurn());
        }
        else if (enemyUnit.currentHP <= 0 || localPlayerUnit.hasWon)
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
        int dealtDamage = preTurnEnemyHealth - enemyUnit.currentHP;
        if (dealtDamage != (int)Math.Round(15 + (15 * (1.0f - preTurnEnemyDef))))
        {
            Debug.Log($"Whirlwind Blade detected as being altered, initiate snapshot {dealtDamage}");
            //RecordState();
            recordThisTurn = true;
            //snapshotManager.initiatedSnapshot = true;
        }
        GameManager.instance.AddToMoveHistory(localPlayerUnit.id, "Whirlwind", "Dealt " + dealtDamage);
        if ((localPlayerUnit.currentHP <= 0 && enemyUnit.currentHP <= 0) || (localPlayerUnit.hasWon && enemyUnit.hasWon))
        {
            state = BattleState.TIE;
            StartCoroutine(EnemyTurn());
        }
        else if (enemyUnit.currentHP <= 0 || localPlayerUnit.hasWon)
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
        int dealtDamage = preTurnEnemyHealth - enemyUnit.currentHP;
        if (dealtDamage != (int)Math.Round(timesHit * (10 + (10 * (1.0f - enemyUnit.defense)))))
        {
            Debug.Log($"Damage that shouldve been dealt with flurry: {timesHit * (10 + (10 * (1.0f - preTurnEnemyDef)))}");
            Debug.Log($"Flurry detected as being altered, initiate snapshot {dealtDamage}");
            //RecordState();
            recordThisTurn = true;
            //snapshotManager.initiatedSnapshot = true;
        }
        GameManager.instance.AddToMoveHistory(localPlayerUnit.id, "Flurry", "Hit " + timesHit + ", dealt " + dealtDamage);
        if ((localPlayerUnit.currentHP <= 0 && enemyUnit.currentHP <= 0) || (localPlayerUnit.hasWon && enemyUnit.hasWon))
        {
            state = BattleState.TIE;
            StartCoroutine(EnemyTurn());
        }
        else if (enemyUnit.currentHP <= 0 || localPlayerUnit.hasWon)
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
        GameManager.instance.AddToMoveHistory(localPlayerUnit.id, "Potion", "Healed 50hp");
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
            if(enemyUnit.currentMove == "Flurry")
            {
                GameManager.instance.AddToMoveHistory(enemyUnit.id, "Flurry", "Hit " + enemyUnit.timesHit + ", dealt " + 0);
            }
            GameManager.instance.AddToMoveHistory(enemyUnit.id, enemyUnit.currentMove, "Dealt " + 0);
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
            GameManager.instance.AddToMoveHistory(enemyUnit.id, "Potion", "Healed 50hp");
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
            int dealtDamage = preTurnLocalHealth - localPlayerUnit.currentHP;
            if (enemyUnit.currentMove == "Flurry")
            {
                if (dealtDamage != enemyUnit.timesHit * (int)Math.Round(10 + (10 * (1.0f - localPlayerUnit.defense))))
                {
                    Debug.Log($"Damage that shouldve been dealt with flurry: {enemyUnit.timesHit * (10 + (10 * (1.0f - localPlayerUnit.defense)))}");
                    Debug.Log($"Enemy's Flurry detected as being altered, initiate snapshot {dealtDamage}");
                    //RecordState();
                    recordThisTurn = true;
                    //snapshotManager.initiatedSnapshot = true;
                }
                GameManager.instance.AddToMoveHistory(enemyUnit.id, "Flurry", "Hit " + enemyUnit.timesHit + ", dealt " + dealtDamage);
            }
            else if(enemyUnit.currentMove == "Protect")
            {
                GameManager.instance.AddToMoveHistory(enemyUnit.id, enemyUnit.currentMove, "Blocked attack.");
            }
            else
            {
                if (enemyUnit.currentMove == "Slash")
                {
                    if(dealtDamage != (int)Math.Round(20 + (20 * (1.0f - localPlayerUnit.defense))))
                    {
                        Debug.Log($"Enemy's {enemyUnit.currentMove} detected as being altered, initiate snapshot {dealtDamage}");
                        //RecordState();
                        recordThisTurn = true;
                        //snapshotManager.initiatedSnapshot = true;
                    }
                }
                else if (enemyUnit.currentMove == "Whirlwind")
                {
                    if (dealtDamage != (int)Math.Round(15 + (15 * (1.0f - preTurnLocalDef))))
                    {
                        Debug.Log($"Enemy's {enemyUnit.currentMove} detected as being altered, initiate snapshot {dealtDamage}");
                        //RecordState();
                        recordThisTurn = true;
                        //snapshotManager.initiatedSnapshot = true;
                    }
                }
                
                GameManager.instance.AddToMoveHistory(enemyUnit.id, enemyUnit.currentMove, "Dealt " + dealtDamage);
            }
            if ((localPlayerUnit.currentHP <= 0 && enemyUnit.currentHP <= 0) || (localPlayerUnit.hasWon && enemyUnit.hasWon))
            {
                state = BattleState.TIE;
                StartCoroutine(EndBattle());
            }
            else if (localPlayerUnit.currentHP <= 0 || enemyUnit.hasWon)
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
            }
            else
            {
                state = BattleState.PLAYERTURN;
                playerAnimator.SetTrigger("EndOfTurn");
                if (recordThisTurn)
                {
                    RecordState();
                    recordThisTurn = false;
                    snapshotManager.initiatedSnapshot = true;
                }
                else
                {
                    PlayerTurn();
                }
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
        dialogue.text = localPlayerUnit.username + " used Protect!";
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
            ClientSend.Marker(snapshot.state);
        }
        else
        {
            Debug.Log("Marker Received, snapshot algorithm complete.");
            // record state of Server -> client channel. In this case, always empty.
            snapshotManager.initiatedSnapshot = false;
        }

    }
}
