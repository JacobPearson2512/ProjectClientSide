using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);

    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myID);
            _packet.Write(UIManager.instance.usernameField.text);
            SendTCPData(_packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.udpTestReceived))
        {
            _packet.Write("Received a UDP packet.");
            SendUDPData(_packet);
        }
    }

    public static void MoveSelected(string _move)
    {
        using (Packet _packet = new Packet((int)ClientPackets.moveSelection))
        {
            _packet.Write(_move);
            SendUDPData(_packet);
        }
    }

    public static void Marker()
    {
        using(Packet _packet = new Packet((int)ClientPackets.marker))
        {
            _packet.Write("Marker arrived, initiate snapshot");
            SendTCPData(_packet);
        }
    }

    public static void SendInitialState(SnapshotRecording.GlobalState _state)
    {
        using(Packet _packet = new Packet((int)ClientPackets.sendInitialState))
        {
            _packet.Write(_state.player1Health);
            _packet.Write(_state.player2Health);
            _packet.Write(_state.player1Defense);
            _packet.Write(_state.player2Defense);
            _packet.Write(_state.player1Potions);
            _packet.Write(_state.player2Potions);
            SendTCPData(_packet);
        }
    }

    public static void SendWinner(int _id)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendWinner))
        {
            _packet.Write(_id);
            SendTCPData(_packet);
        }
    }

    public static void SendFinalState(SnapshotRecording.GlobalState _state)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendFinalState))
        {
            _packet.Write(_state.player1Health);
            _packet.Write(_state.player2Health);
            _packet.Write(_state.player1Defense);
            _packet.Write(_state.player2Defense);
            _packet.Write(_state.player1Potions);
            _packet.Write(_state.player2Potions);
            SendTCPData(_packet);
        }
    }

    public static void SendMoveHistory(List<MoveHistoryEntry> _history)
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendMoveHistory))
        {
            _packet.Write(_history.Count);
            foreach (MoveHistoryEntry _entry in _history)
            {
                _packet.Write(_entry.playerID);
                _packet.Write(_entry.actionName);
                _packet.Write(_entry.actionEffect);
            }
            SendTCPData( _packet);
        }
    }
    #endregion
}

