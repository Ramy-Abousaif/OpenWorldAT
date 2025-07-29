using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public BoxCollider swordCol;

    public void SwordActive()
    {
        swordCol.enabled = true;
    }

    public void SwordInactive()
    {
        swordCol.enabled = false;
    }
}
