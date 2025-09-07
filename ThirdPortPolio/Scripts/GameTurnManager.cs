using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTurnManager : MonoBehaviour
{
    public static GameTurnManager Instance;
    public event Action<int> OnCostChanged; // ���� �ڿ��� �����ϴ� �̺�Ʈ
    private int _currentCost = 0;
    public int CurrentCost
    {
        get => _currentCost;
        set
        {
            _currentCost = value;
            OnCostChanged?.Invoke(_currentCost); // �ڿ��� ����� ������ �̺�Ʈ �߻�
        }
    }

    public void TurnStart(S_TurnStart packet)
    {
        packet.turnNumber++;
        packet.turnTime = 0;
        _currentCost++;
    }

    public void TurnEnd()
    {
        
    }

}
