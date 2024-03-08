using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public BattleSystem battleSystem;

    public static Dictionary<int, Unit> players = new Dictionary<int, Unit>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    public Transform playerBattleLocation;
    public Transform enemyBattleLocation;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, int _maxHP, int _numberPotions)
    {
        GameObject _player;
        if (_id == Client.instance.myID)
        {
            _player = Instantiate(localPlayerPrefab, playerBattleLocation);
        }
        else
        {
            _player = Instantiate(playerPrefab, enemyBattleLocation);
        }

        _player.GetComponent<Unit>().id = _id;
        _player.GetComponent<Unit>().username = _username;
        _player.GetComponent<Unit>().maxHP = _maxHP;
        _player.GetComponent<Unit>().currentHP = _maxHP;
        players.Add(_id, _player.GetComponent<Unit>());

    }

    public void StartBattleSystem()
    {
        Debug.Log("Starting battle!");
        battleSystem.StartBattleSystem();
    }

    public void PlayTurn(string _move)
    {
        switch (_move) 
        {
            case "Slash":
                StartCoroutine(battleSystem.Slash());
                break;
            case "Protect":
                StartCoroutine(battleSystem.Block());
                break;
            case "Whirlwind":
                StartCoroutine(battleSystem.Whirlwind());
                break;
            case "Heal":
                StartCoroutine(battleSystem.PlayerBag());
                break;
            case "Flurry":
                StartCoroutine(battleSystem.Flurry());
                break;
        }

    }
}
