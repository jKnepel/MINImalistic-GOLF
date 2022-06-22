using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//container class for all records saved

[System.Serializable]
public class Records {
    [SerializeField] public List<Record> records;
}
