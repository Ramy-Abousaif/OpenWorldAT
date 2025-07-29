using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goal
{
    public GoalType goalType;

    public int requiredAmt;
    public int CurrentAmt;

    public bool IsReached()
    {
        return (CurrentAmt >= requiredAmt);
    }

    public void EnemyKilled()
    {
        if (goalType == GoalType.KILL)
            CurrentAmt++;
    }

    public void ItemCollected()
    {
        if (goalType == GoalType.GATHER)
            CurrentAmt++;
    }
}

public enum GoalType
{
    KILL,
    GATHER
}
