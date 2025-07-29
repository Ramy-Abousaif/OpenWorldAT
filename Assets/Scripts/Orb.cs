using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Orb : MonoBehaviour
{
    public int id;
    public bool active;
    public Vector3 position;
    public Vector3 scale;

    [System.Serializable]
    public class SaveToken
    {
        public int id;
        public bool active;
        public Vector3 position;
        public Vector3 scale;
    }

    public SaveToken Tokenize()
    {
        var token = new SaveToken();
        token.id = this.id;
        token.active = this.active;
        token.position = this.transform.position;
        token.scale = this.transform.localScale;
        return token;
    }
}
