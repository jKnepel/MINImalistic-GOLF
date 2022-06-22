using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {
    public GameController gameController;
    public Transform player;
    public GameObject cameraOrbit;

    public float rotationSpeed;
    private float scrollSpeed = 10;
    private float zoom = 25;

    public bool move;

    void LateUpdate() {
        //update viewspeed
        rotationSpeed = gameController.inputManager.viewspeed;

        //move camera with player
        cameraOrbit.transform.position = player.position;

        //move camera if player is not charging
        if (move) {
            Vector3 input = new Vector3(rotationSpeed * Input.GetAxis("Mouse X"), rotationSpeed * Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel"));

            //rotate orbit around player depending on mouse position and scale orbit with scrollvalue
            cameraOrbit.transform.rotation = Quaternion.Euler(Mathf.Clamp(cameraOrbit.transform.eulerAngles.x - input.y, 0, 50), cameraOrbit.transform.eulerAngles.y + input.x, 0);
            zoom = Mathf.Clamp(zoom - input.z * scrollSpeed, 30, 80);
            cameraOrbit.transform.localScale = new Vector3(zoom, zoom, zoom);
        }

        //rotate camera to look at player
        transform.LookAt(player.position);
    }
}