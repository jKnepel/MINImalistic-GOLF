using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public Player player;
    public string playerName;

    //gameUI
    public GameObject gameUI;
    public TMPro.TextMeshProUGUI strokesUI;
    public TMPro.TextMeshProUGUI timerUI;
    public TMPro.TextMeshProUGUI parMSG;
    public GameObject scoreboard;
    public GameObject pauseCamera;

    //pauseMenu
    public GameObject pauseMenu;
    public Button statisticsButton;
    public GameObject statistics;
    public GameObject options;
    public InputManager inputManager;
    public Transform keybindings;

    //game Info
    public Hole[] holes;
    public int strokeTotal = 0;
    private int currentHole = 0;
    private float timer;

    public bool pause = false;
    private bool midbreak;

    //path for record and options savegame
    private string savePath;
    private string optionsPath;

    void Awake() {
        //options
        optionsPath = Path.Combine(Application.persistentDataPath, "minimGolf.options");

        //read input-manager from file or create new one
        if (File.Exists(optionsPath)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamOpen = new FileStream(optionsPath, FileMode.Open);
            inputManager = formatter.Deserialize(streamOpen) as InputManager;
            streamOpen.Close();

            //set input-manager options values
            options.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<Slider>().value = inputManager.viewspeed;
            keybindings.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.hit;
            keybindings.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.abort;
            keybindings.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.resetLast;
            keybindings.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.resetFirst;
            keybindings.GetChild(6).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.pause;
        }

        //statistics
        savePath = Path.Combine(Application.persistentDataPath, "course1Records.savegame");

        //read records and set statistics window or grey out statistics button 
        if (File.Exists(savePath)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamOpen = new FileStream(savePath, FileMode.Open);
            Records records = formatter.Deserialize(streamOpen) as Records;
            streamOpen.Close();

            int rows = 0;
            foreach (Record record in records.records) {
                rows++;

                //set row position
                Transform row = Instantiate(Resources.Load<GameObject>("UI/StatisticsRow")).transform;
                row.SetParent(statistics.transform.GetChild(0).GetChild(1));
                row.localScale = new Vector3(1, 1, 1);
                row.localRotation = Quaternion.identity;
                row.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                row.GetComponent<RectTransform>().offsetMax = new Vector3(0, 50);
                row.localPosition = Vector3.zero;
                row.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -65 + (-65 * rows));
                statistics.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 50);

                //set row values
                row.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + rows + ". " + record.playerName;
                for (int i = 0; i < 9; i++) row.GetChild(i + 1).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "" + record.strokes[i];
                row.GetChild(10).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + record.strokeTotal;
            }
        } else statisticsButton.interactable = false;
    }

    void Start() {
        //set player name in scoreboard
        playerName = GameObject.Find("NamePasser").GetComponent<NamePasser>().name;
        scoreboard.transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + playerName;

        //set par values for course in scoreboard
        int parTotal = 0;
        for (int i = 0; i < holes.Length; i++) {
            scoreboard.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(i + 1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + holes[i].par;
            statistics.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(i + 1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + holes[i].par;
            parTotal += holes[i].par;
        }
        scoreboard.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(10).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + parTotal;
        statistics.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(10).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + parTotal;

        //load first hole
        LoadNextHole();
    }

    void Update() {
        //update values while playing
        if (!pause && !midbreak) {
            //timer countdown
            timer -= Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(timer);
            timerUI.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            if (timer <= 0 && Time.timeScale != 0 && !pause) {
                player.strokes = 13;
                ReachedGoal();
            }

            //set number of strokes
            strokesUI.text = "" + player.strokes;

            //show scoreboard
            if (Input.GetKeyDown(KeyCode.Tab)) scoreboard.SetActive(true);
            else if (Input.GetKeyUp(KeyCode.Tab)) scoreboard.SetActive(false);
        }

        //show/close pausemenu
        if (Input.GetKeyDown(inputManager.pause)) {
            if (pause) EndPause();
            else StartPause();
        }
    }

    //called upon reaching the hole
    public void ReachedGoal() {
        //update stroke values
        holes[currentHole - 1].strokes = player.strokes;
        scoreboard.transform.GetChild(0).GetChild(1).GetChild(2).GetChild(currentHole).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + player.strokes;
        strokeTotal += player.strokes;
        scoreboard.transform.GetChild(0).GetChild(1).GetChild(2).GetChild(10).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + strokeTotal;

        //show par Message
        if (player.strokes == 1) parMSG.text = "Hole-in-One";
        else {
            switch (holes[currentHole - 1].par - player.strokes) {
                case 4: parMSG.text = "Condor"; break;
                case 3: parMSG.text = "Albatros"; break;
                case 2: parMSG.text = "Eagle"; break;
                case 1: parMSG.text = "Birdie"; break;
                case 0: parMSG.text = "Par"; break;
                case -1: parMSG.text = "Bogey"; break;
                case -2: parMSG.text = "Double Bogey"; break;
                case -3: parMSG.text = "Triple Bogey"; break;
                default: if ((holes[currentHole - 1].par - player.strokes) < 0) parMSG.text = ((holes[currentHole - 1].par - player.strokes)*-1) + " over Par";
                         else parMSG.text = (holes[currentHole - 1].par - player.strokes) + " under Par"; break;
            }
        }

        IEnumerator parMsg = ShowParMessage();
        StartCoroutine(parMsg);
    }

    //coroutine to show par Message and start midbreak when its finished
    private IEnumerator ShowParMessage() {
        Time.timeScale = 0;
        parMSG.gameObject.SetActive(true);
        if (scoreboard.activeSelf) scoreboard.SetActive(false);
        
        for (float i = 0; i < 3; i += Time.unscaledDeltaTime) yield return null;
        
        parMSG.gameObject.SetActive(false);
        StartBreak();
    }

    //start break between holes
    private void StartBreak() {
        midbreak = true;
        SetPauseCamera();

        //update gameUI
        scoreboard.SetActive(true);
        gameUI.transform.GetChild(0).gameObject.SetActive(false);

        //show button for next hole or end game
        if (currentHole < holes.Length) scoreboard.transform.GetChild(1).gameObject.SetActive(true);
        else scoreboard.transform.GetChild(2).gameObject.SetActive(true);
    }

    //end break and load next hole on button press
    private void EndBreak() {
        Time.timeScale = 1;
        midbreak = false;
        ResetPauseCamera();

        //reset gameUI
        scoreboard.SetActive(false);
        scoreboard.transform.GetChild(1).gameObject.SetActive(false);
        gameUI.transform.GetChild(0).gameObject.SetActive(true);

        //destroy last hole for better performance
        Destroy(holes[currentHole - 1].gameObject);

        LoadNextHole();
    }

    //open pausemenu
    private void StartPause() {
        pause = true;

        //update gameUI
        scoreboard.SetActive(false);
        scoreboard.transform.GetChild(1).gameObject.SetActive(false);
        Time.timeScale = 0;
        
        if(!midbreak) SetPauseCamera();
        
        pauseMenu.SetActive(true);
    }

    //close pausemenu
    public void EndPause() {
        pause = false;

        statistics.SetActive(false);
        options.SetActive(false);

        //reset gameUI or change it to breakUI
        if(midbreak) {
            scoreboard.SetActive(true);
            gameUI.transform.GetChild(0).gameObject.SetActive(false);
            if (currentHole < holes.Length) scoreboard.transform.GetChild(1).gameObject.SetActive(true);
            else scoreboard.transform.GetChild(2).gameObject.SetActive(true);
        } else {
            Time.timeScale = 1;
            ResetPauseCamera();
            gameUI.transform.GetChild(0).gameObject.SetActive(true);
        }
        
        pauseMenu.SetActive(false);
    }

    //set camera to rotating pausecamera
    private void SetPauseCamera() {
        player.transform.parent.gameObject.SetActive(false);
        pauseCamera.GetComponent<PauseCameraBehaviour>().target = player.transform;
        pauseCamera.transform.position = new Vector3(0, player.transform.position.y + 20, player.transform.position.z - 20);
        pauseCamera.SetActive(true);
    }

    //reset camera to normal game camera
    private void ResetPauseCamera() {
        player.transform.parent.gameObject.SetActive(true);
        pauseCamera.SetActive(false);
    }

    //load the next hole
    private void LoadNextHole() {
        //load next hole
        currentHole++;
        holes[currentHole - 1].gameObject.SetActive(true);
        scoreboard.transform.GetChild(0).GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Hole: " + currentHole + " / 9";

        //set player values
        player.startPos = holes[currentHole - 1].startPos;
        player.lastPos = holes[currentHole - 1].startPos;
        player.ResetPlayer(player.startPos);
        player.strokes = 0;

        timer = 180;
        Time.timeScale = 1;
    }

    //save record if better than current 5 records and leave game
    private void EndGame() {
        Record newRecord = new Record(this);

        if (File.Exists(savePath)) {
            //open statistics file
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamOpen = new FileStream(savePath, FileMode.Open);
            Records records = formatter.Deserialize(streamOpen) as Records;
            streamOpen.Close();

            //add record if its better than any of the 5 current records
            records.records.Add(newRecord);
            records.records.Sort((p1, p2) => p1.strokeTotal.CompareTo(p2.strokeTotal));
            if (records.records.Count > 5) records.records.RemoveAt(5);

            //save records
            FileStream streamCreate = new FileStream(savePath, FileMode.Create);
            formatter.Serialize(streamCreate, records);
            streamCreate.Close();
        } else {
            //create records and add new record
            Records records = new Records();
            records.records = new List<Record>();
            records.records.Add(newRecord);

            //save record
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamCreate = new FileStream(savePath, FileMode.Create);
            formatter.Serialize(streamCreate, records);
            streamCreate.Close();
        }

        //return to mainmenu
        SceneManager.LoadScene("GameMenu");
    }


    //----------------------Statistics---------------------------

    //open statistics window
    public void Statistics() {
        pauseMenu.SetActive(false);
        statistics.SetActive(true);
    }

    //close statistics window
    public void StatisticsLeave() {
        statistics.SetActive(false);
        pauseMenu.SetActive(true);
    }

    //----------------------Options---------------------------

    //open options window
    public void Options() {
        pauseMenu.SetActive(false);
        options.SetActive(true);
    }

    //close options window and save options
    public void OptionsLeave() {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream streamCreate = new FileStream(optionsPath, FileMode.Create);
        formatter.Serialize(streamCreate, inputManager);
        streamCreate.Close();

        options.SetActive(false);
        pauseMenu.SetActive(true);
    }

    //change keybindings for button
    public void ChangeKey() {
        try {
            //get chosen input option
            Transform button; int type;
            switch (EventSystem.current.currentSelectedGameObject.name) {
                case "Hit": button = keybindings.GetChild(2).GetChild(1); type = 0; break;
                case "Abort": button = keybindings.GetChild(3).GetChild(1); type = 1; break;
                case "Reset to last Position": button = keybindings.GetChild(4).GetChild(1); type = 2; break;
                case "Reset to first Position": button = keybindings.GetChild(5).GetChild(1); type = 3; break;
                case "PauseMenu": button = keybindings.GetChild(6).GetChild(1); type = 4; break;
                default: return;
            }

            //start pressed input routine
            button.GetComponent<Button>().interactable = false;
            IEnumerator coroutine = GetInput(button, type);
            StartCoroutine(coroutine);
        } catch (NullReferenceException) { return; }
    }

    //coroutine to get the next input and check if its a correct input
    private IEnumerator GetInput(Transform button, int type) {
        while (!Input.anyKeyDown) yield return null;

        KeyCode key = KeyCode.None;
        foreach (KeyCode _key in System.Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKey(_key)) key = _key;

        if (CheckViable(key) && !CheckDouble(key)) {
            button.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + key;
            //set input manager
            switch (type) {
                case 0: inputManager.hit = key; break;
                case 1: inputManager.abort = key; break;
                case 2: inputManager.resetLast = key; break;
                case 3: inputManager.resetFirst = key; break;
                case 4: inputManager.pause = key; break;
            }
        }
        button.GetComponent<Button>().interactable = true;
    }

    //check if key is a usable key
    private bool CheckViable(KeyCode key) {
        String keyString = key.ToString();
        if (keyString == "Space" || keyString == "Escape" || keyString == "Mouse0" || keyString == "Mouse1") return true;
        else if (keyString[0] >= 65 && keyString[0] <= 90 && keyString.Length == 1) return true;
        else return true;
    }

    //check if key is already in use
    private bool CheckDouble(KeyCode key) {
        if (keybindings.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) return true;
        else if (keybindings.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) return true;
        else if (keybindings.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) return true;
        else if (keybindings.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) return true;
        else if (keybindings.GetChild(6).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == key.ToString()) return true;
        return false;
    }

    //set view speed with slider
    public void SetViewSpeed(float sliderValue) { inputManager.viewspeed = sliderValue; }

    //----------------------------------------------------

    //return to mainmenu
    public void Leave() { SceneManager.LoadScene("GameMenu"); }
}
