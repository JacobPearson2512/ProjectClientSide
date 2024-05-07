using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Text;
using static SnapshotRecording;

public class ClientHandle : MonoBehaviour
{
    public static BattleSystem battleSystem;
    static bool consensusPacketReceived = false;

    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _id = _packet.ReadInt();
        bool _useInjection = _packet.ReadBool();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myID = _id;
        ClientSend.WelcomeReceived();
        Client.instance.useInjection = _useInjection;
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        int _maxHP = _packet.ReadInt();
        int _numberPotions = _packet.ReadInt();

        GameManager.instance.SpawnPlayer(_id, _username, _maxHP, _numberPotions);
    }

    public static void StartBattle(Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"Message from server: {_msg}");
        UIManager.instance.waitScreen.SetActive(false);
        UIManager.instance.battleUI.SetActive(true);
        GameManager.instance.StartBattleSystem();
    }

    public static void UpdatePlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        GameManager.players[_id].currentHP = _packet.ReadInt();
        GameManager.players[_id].numberPotions = _packet.ReadInt();
        GameManager.players[_id].defense = _packet.ReadFloat();
        GameManager.players[_id].currentMove = _packet.ReadString();
        GameManager.players[_id].timesHit = _packet.ReadInt();
        GameManager.players[_id].hasWon = _packet.ReadBool();
        if (_id == Client.instance.myID)
        {
            GameManager.instance.battleSystem.newTurn = false;
            consensusPacketReceived = false;
            GameManager.instance.PlayTurn(GameManager.players[_id].currentMove);
        }
    }

    public static void MarkerRecieved(Packet _packet)
    {
        Debug.Log(_packet.ReadString());
        GameManager.instance.battleSystem.RecordState();
    }

    public static void Consensus(Packet _packet)
    {
        Debug.Log("Consensus Packet Received");
        if (consensusPacketReceived)
        {
            return;
        }
        if (GameManager.instance.battleSystem.newTurn)
        {
            return;
        }
        if(Client.instance.myID == 1)
        {
            GameManager.players[1].currentHP = _packet.ReadInt();
            GameManager.players[2].currentHP = _packet.ReadInt();
            GameManager.players[1].defense = _packet.ReadFloat();
            GameManager.players[2].defense = _packet.ReadFloat();
            GameManager.players[1].numberPotions = _packet.ReadInt();
            GameManager.players[2].numberPotions = _packet.ReadInt();
        }
        else
        {
            GameManager.players[2].currentHP = _packet.ReadInt();
            GameManager.players[1].currentHP = _packet.ReadInt();
            GameManager.players[2].defense = _packet.ReadFloat();
            GameManager.players[1].defense = _packet.ReadFloat();
            GameManager.players[2].numberPotions = _packet.ReadInt();
            GameManager.players[1].numberPotions = _packet.ReadInt();
        }
        GameManager.instance.battleSystem.ResolveMoveHistory();
        consensusPacketReceived = true;
        GameManager.instance.battleSystem.PlayerTurn();

    }
}
