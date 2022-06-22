using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCameraBehaviour : MonoBehaviour {
    public Transform target;

    //rotate camera around target with unscaled time
    void Update() {
        transform.RotateAround(target.transform.position, Vector3.up, 20 * Time.unscaledDeltaTime);
        transform.LookAt(target);
    }
}
