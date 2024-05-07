using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SnapshotRecording : MonoBehaviour
{
    public class GlobalState 
    {
        public int player1Health;
        public int player2Health;
        public float player1Defense;
        public float player2Defense;
        public int player1Potions;
        public int player2Potions;

        public GlobalState(int _player1Health, int _player2Health, float _player1Defense, float _player2Defense, int _player1Potions, int _player2Potions)
        {
            player1Health = _player1Health;
            player2Health = _player2Health;
            player1Defense = _player1Defense;
            player2Defense = _player2Defense;
            player1Potions = _player1Potions;
            player2Potions = _player2Potions;
        }
    }

    public class Snapshot
    {
        public int snapshotId;
        public GlobalState state;

        public Snapshot(int _snapshotId, GlobalState _state)
        {
            snapshotId = _snapshotId;
            state = _state;
        }
    }

    public class SnapshotManager
    {
        public bool initiatedSnapshot = false;
        private List<Snapshot> snapshots = new List<Snapshot>();

        // Method to initiate a snapshot
        public Snapshot TakeSnapshot(int snapshotId, GlobalState state)
        {
            Snapshot snapshot = new Snapshot(snapshotId, state);
            snapshots.Add(snapshot);
            return snapshot;
        }
    }


}
