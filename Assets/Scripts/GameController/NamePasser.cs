using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePasser : MonoBehaviour {
    public new string name;
    void Awake() { DontDestroyOnLoad(this.gameObject); }
}
