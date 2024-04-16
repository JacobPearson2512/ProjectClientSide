using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InconsistencyInjection
{
    public int AlterWinner(int _winner)
    {
        if (Random.Range(0, 2) == 1)
        {
            if (_winner == 1)
            {
                _winner = 2;
            }
            else
            {
                _winner = 1;
            }
        }
        return _winner;
    }
}
