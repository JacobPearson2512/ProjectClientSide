using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _id = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myID = _id;
        ClientSend.WelcomeReceived();

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
    }
}
