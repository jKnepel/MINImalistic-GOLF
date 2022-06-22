using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Water Shader by https://www.youtube.com/watch?v=Vg0L9aCRWPE
public class WaterBehaviour : MonoBehaviour {
    //test if player touches water and reset him
    void OnTriggerEnter(Collider other) {
        Player player = other.gameObject.GetComponent<Player>();
        if (player != null) player.ResetPlayer(player.lastPos);
    }
}
