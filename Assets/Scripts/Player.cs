using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    public GameController gameController;
    public CameraBehaviour cameraScript;
    private Rigidbody rb;

    private bool charge = false;
    private float power = 0;
    private bool accelerate = false;
    public RectTransform powerMeter;
    public Transform direction;

    public Vector3 lastPos;
    public Vector3 startPos;

    public int strokes = 0;

    void Awake() { rb = gameObject.GetComponent<Rigidbody>(); }

    void Update() {
        if (rb.velocity.magnitude == 0) {
            //load next hole when too many strokes have been reached
            if(strokes >= 12 && Time.timeScale != 0 && !gameController.pause) {
                strokes = 13;
                gameController.ReachedGoal();
                return;
            }

            //direction visualizer
            if (Time.timeScale != 0) {
                direction.gameObject.SetActive(true);
                direction.position = new Vector3(transform.position.x, transform.position.y - 0.49f, transform.position.z);
                Quaternion targetRotation = Quaternion.Euler(0, cameraScript.transform.eulerAngles.y, 0);
                direction.rotation = Quaternion.RotateTowards(direction.rotation, targetRotation, 1000 * Time.deltaTime);
            } else direction.gameObject.SetActive(false);


            //get mouse inputs and start/stop power charge
            if (Input.GetKeyDown(gameController.inputManager.hit)) {
                cameraScript.move = false;
                charge = true;
            } else if (Input.GetKeyUp(gameController.inputManager.hit) && charge == true) {
                accelerate = true;
            } else if (Input.GetKeyDown(gameController.inputManager.abort) && charge == true) {
                cameraScript.move = true;
                charge = false;
                power = 0;
                powerMeter.sizeDelta = new Vector2(60, 0);
            }

            //charge power
            if (charge) {
                power = Mathf.Clamp(power += Input.GetAxis("Mouse Y"), 0, 400);
                powerMeter.sizeDelta = new Vector2(60, Mathf.Lerp(0, 470, power / 400));
            }
        } else direction.gameObject.SetActive(false);

        //reset player
        if (Input.GetKeyDown(gameController.inputManager.resetLast)) ResetPlayer(lastPos);
        else if (Input.GetKeyDown(gameController.inputManager.resetFirst)) {
            ResetPlayer(startPos);
            lastPos = startPos;
        }

        //stop player below certain speed to prevent sliding
        if (rb.velocity.magnitude <= 0.5f && rb.velocity.magnitude >= 0.4f) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate() {
        //accelerate ball on hit
        if (accelerate) {
            if(power > 10) {
                rb.AddForce(new Vector3(cameraScript.transform.forward.x, 0, cameraScript.transform.forward.z) * power * 200);
                strokes++;
                lastPos = transform.position;
            }
            
            accelerate = false;
            cameraScript.move = true;
            charge = false;
            power = 0;
            powerMeter.sizeDelta = new Vector2(60, 0);
        }
    }

    public void ResetPlayer(Vector3 pos) {
        rb.MovePosition(new Vector3(pos.x, pos.y + 1, pos.z));
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
