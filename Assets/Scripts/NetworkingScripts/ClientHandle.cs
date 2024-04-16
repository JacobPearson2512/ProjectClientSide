using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Text;
using static SnapshotRecording;

public class ClientHandle : MonoBehaviour
{
    public static BattleSystem battleSystem;

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

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Received UDP packet: {_msg}");
        ClientSend.UDPTestReceived();
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
            GameManager.instance.PlayTurn(GameManager.players[_id].currentMove);
        }
    }

    public static void JsonResult(Packet _packet)
    {
        string _jsonData = _packet.ReadString();
        Unit testUnit = JsonUtility.FromJson<Unit>(_jsonData);
        Debug.Log(testUnit.ToString());
    }

    public static void MarkerRecieved(Packet _packet)
    {
        Debug.Log(_packet.ReadString());
        GameManager.instance.battleSystem.RecordState();
        // Record state, send marker. Unless it initiated.
        /*if (battleSystem.snapshotManager.initiatedSnapshot)
        {
            // record state of Server -> client channel. In this case, always empty.
        }
        else
        {
            battleSystem.RecordState(); // Sends Marker.
        }*/
    }
}
