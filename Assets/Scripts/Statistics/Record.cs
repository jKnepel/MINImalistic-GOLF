using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Record {
    public string playerName;
    public int strokeTotal;
    public int[] strokes;

    //record constructor to set values of new record
    public Record(GameController controller) {
        playerName = controller.playerName;
        strokeTotal = controller.strokeTotal;
        strokes = new int[controller.holes.Length];
        for (int i = 0; i < controller.holes.Length; i++)
            strokes[i] = controller.holes[i].strokes;
    }
}
