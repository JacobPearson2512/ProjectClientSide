using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHistoryEntry
{
    public int playerID;
    public string actionName;
    public string actionEffect;

    public MoveHistoryEntry(int _player, string _action, string _effect)
    {
        playerID = _player;
        actionName = _action;
        actionEffect = _effect;
    }
}
