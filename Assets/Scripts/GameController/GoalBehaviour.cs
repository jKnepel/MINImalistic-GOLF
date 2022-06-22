using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour {
    public GameController gameController;
    private Transform player;
    private float playerDistance;
    public Transform flag;

    void Awake() { player = GameObject.Find("Player").transform; }

    void Update() {
        //raise flag depending on player distance
        playerDistance = Vector3.Distance(player.position, transform.position);
        flag.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 3, 0), 1 / playerDistance);
    }

    //check if player has stopped in hole
    void OnTriggerStay(Collider other) {
        Player player = other.gameObject.GetComponent<Player>();
        if(player != null)
            if (player.gameObject.GetComponent<Rigidbody>().velocity.magnitude <= 0.5f && Time.timeScale != 0 && !gameController.pause)
                gameController.ReachedGoal();
    }
}
