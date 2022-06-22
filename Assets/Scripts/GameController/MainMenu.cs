using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TMPro;

public class MainMenu : MonoBehaviour {
    public GameObject buttonsMenu;

    public GameObject startGame; //window for choosing the name
    public TMPro.TMP_InputField inputField;

    public GameObject statistics; //statistics window
    public Button statisticsButton;
    public GameObject options; //options window
    public InputManager inputManager; //input-manager to define keys
    public Transform keybindings; 

    public NamePasser namePasser; //object for saving the name between scenes

    private string optionsPath; //path for options savefile

    void Awake() {
        //options
        optionsPath = Path.Combine(Application.persistentDataPath, "minimGolf.options");

        //read input-manager from file or create new one
        if (File.Exists(optionsPath)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamOpen = new FileStream(optionsPath, FileMode.Open);
            inputManager = formatter.Deserialize(streamOpen) as InputManager;
            streamOpen.Close();
        } else {
            inputManager = new InputManager();
            inputManager.viewspeed = 3;
            inputManager.hit = KeyCode.Mouse0;
            inputManager.abort = KeyCode.Mouse1;
            inputManager.resetLast = KeyCode.F;
            inputManager.resetFirst = KeyCode.R;
            inputManager.pause = KeyCode.Escape;

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream streamCreate = new FileStream(optionsPath, FileMode.Create);
            formatter.Serialize(streamCreate, inputManager);
            streamCreate.Close();
        }

        //set input-manager options values
        options.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<Slider>().value = inputManager.viewspeed;
        keybindings.GetChild(2).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.hit;
        keybindings.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.abort;
        keybindings.GetChild(4).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.resetLast;
        keybindings.GetChild(5).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.resetFirst;
        keybindings.GetChild(6).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + inputManager.pause;

        //statistics
        string savePath = Path.Combine(Application.persistentDataPath, "course1Records.savegame");

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

    //check if name was chosen
    void Update() {
        if (inputField.text == "") startGame.transform.GetChild(1).GetComponent<Button>().interactable = false;
        else startGame.transform.GetChild(1).GetComponent<Button>().interactable = true;
    }

    //open name window
    public void NewGame() {
        buttonsMenu.SetActive(false);
        startGame.SetActive(true);
    }

    //start game
    public void ConfirmNewGame() {
        namePasser.name = inputField.text;        
        SceneManager.LoadScene("Course1");
    }

    //return to main menu
    public void AbortNewGame() {
        startGame.SetActive(false);
        buttonsMenu.SetActive(true);
    }

    //open statistics
    public void Statistics() {
        buttonsMenu.SetActive(false);
        statistics.SetActive(true);
    }

    //return to main menu
    public void BackStatistics() {
        statistics.SetActive(false);
        buttonsMenu.SetActive(true);
    }

    //----------------------Options---------------------------

    //open options menu
    public void Options() {
        buttonsMenu.SetActive(false);
        options.SetActive(true);
    }

    //return to main menu and save options
    public void OptionsLeave() {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream streamCreate = new FileStream(optionsPath, FileMode.Create);
        formatter.Serialize(streamCreate, inputManager);
        streamCreate.Close();

        options.SetActive(false);
        buttonsMenu.SetActive(true);
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

    //coroutine to get the next input and check if its in a correct format
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

    //-------------------------------------------------

    //close game
    public void ExitGame() { Application.Quit(); }
}
