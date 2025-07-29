using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public bool isActive;

    public string title;
    public string description;
    public string finishedDescription;
    public int xpReward;
    public int goldReward;

    public Goal goal;

    public void Complete()
    {
        isActive = false;
    }
}
