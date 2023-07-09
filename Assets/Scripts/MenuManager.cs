using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public MenuManager Instance;
    public Transform MenuButtons;
    public Transform LevelsPopup;
    public Transform CelebrationParticles;
    public Transform CelebrationCanvas;

    private void Awake()
    {
        Instance = this;
        if (!PlayerPrefs.HasKey("last_unlocked_level"))
        {
            PlayerPrefs.SetInt("last_unlocked_level", 3);//initially there are 3 unlocked levels
            PlayerPrefs.Save();
        }
        InitialLevelsPopupSetup();
    }

    private void Start()
    {
        int lastLevel = PlayerPrefs.GetInt("last_played_level");
        int maxScore = PlayerPrefs.GetInt("max_score_has_changed_level_" + lastLevel.ToString());
        Debug.Log("level " + lastLevel.ToString() + " score was " + PlayerPrefs.GetInt("max_score_level_" + lastLevel.ToString()));
        if (maxScore != 0)//if max scored achieved on recently played level
        {
            StartCoroutine(PlayMaxScoreAnimation(maxScore));
            UnlockNewLevel();
            PlayerPrefs.SetInt("max_score_has_changed_level_" + lastLevel.ToString(), 0);//bring it to a neutral state
            UpdateLevels(PlayerPrefs.GetInt("last_unlocked_level"), lastLevel, maxScore);
        }
    }

    public IEnumerator PlayMaxScoreAnimation(int maxScore)//for 3 seconds
    {
        Debug.Log("plaiying max score animation");
        //changing the high score text
        CelebrationCanvas.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = maxScore.ToString();
        LevelsPopup.gameObject.SetActive(false);

        CelebrationParticles.gameObject.SetActive(true);
        CelebrationCanvas.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5);

        CelebrationCanvas.gameObject.SetActive(false);
        CelebrationParticles.gameObject.SetActive(false);

        MenuButtons.gameObject.SetActive(false);
        LevelsPopup.gameObject.SetActive(true);
    }

    public void UnlockNewLevel()
    {
        if (!PlayerPrefs.HasKey("last_unlocked_level"))
        {
            PlayerPrefs.SetInt("last_unlocked_level", 4);
        }
        else
        {
            PlayerPrefs.SetInt("last_unlocked_level", PlayerPrefs.GetInt("last_unlocked_level")+1);//bunu düzelt, kod düzgün çalışmadığı için bozulmuş
        }
        PlayerPrefs.Save();
        Debug.Log("last unlocked level has changed to " + PlayerPrefs.GetInt("last_unlocked_level"));
    }

    #region button triggered methods
    public void OpenLevesPopup()
    {
        MenuButtons.gameObject.SetActive(false);
        LevelsPopup.gameObject.SetActive(true);
    }

    public void CloseLevesPopup()
    {
        LevelsPopup.gameObject.SetActive(false);
        MenuButtons.gameObject.SetActive(true);
    }

    public void LoadLevel(int level)//if level is loadable
    {
        if (PlayerPrefs.GetInt("last_unlocked_level") < level) 
        {
            Debug.Log(level.ToString()+" is not unlocked yet, last unlocked level is " + PlayerPrefs.GetInt("last_unlocked_level").ToString());
            return;
        }
        if(!File.Exists(Application.persistentDataPath+ "/Levels/RM_A" + level.ToString()))
        {
            string log = "Level " + level.ToString() + " is unlocked but level info couldn't be downloaded, please make sure you are connected to internet";
            Debug.Log(log);
            return;
        }
        Debug.Log("Trying to load unlocked level: "+level.ToString());
        MenuLevelManager.Instance.SetPalyerPrefsForLevel(level);
        SceneManager.LoadScene(1);//loading the GameScene
    }
    
    #endregion

    

    public void UpdateLevels(int lastUnlockedLevel, int lastPlayedLevel, int maxScore)//update levels popup: max scores, new unlocks
    {
        Transform levelButtons = LevelsPopup.GetChild(0).GetChild(0);
        levelButtons.GetChild(lastPlayedLevel - 1).GetChild(1).GetComponent<TextMeshProUGUI>().text
            = "Level " + lastPlayedLevel.ToString() + " - " + PlayerPrefs.GetInt("move_count_" + lastPlayedLevel.ToString()) + " Moves";
        levelButtons.GetChild(lastPlayedLevel - 1).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Highest Score: "+ maxScore.ToString();
        levelButtons.GetChild(lastUnlockedLevel - 1).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/play");
    }

    public void InitialLevelsPopupSetup()
    {
        Transform levelButtons = LevelsPopup.GetChild(0).GetChild(0);
        int lastUnlocked = PlayerPrefs.GetInt("last_unlocked_level");
        for(int i = lastUnlocked; i< levelButtons.childCount; i++)//beyond last unlocked level
        {
            levelButtons.GetChild(i).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/locked");
            if (File.Exists(Application.persistentDataPath + "/Levels/RM_A" + (i+1).ToString()))
            {
                levelButtons.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text
                    = "Level " + (i+1).ToString() + " - " + PlayerPrefs.GetInt("move_count_" + (i + 1).ToString()) + " Moves";
                levelButtons.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Highest Score: " +
                    PlayerPrefs.GetInt("max_score_level_" + (i + 1).ToString()).ToString();
                levelButtons.GetChild(i).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/locked");
            }
            else
            {
                levelButtons.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Level " + (i + 1).ToString()+" is Unknown";
                levelButtons.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Highest Score: Unknown";
                levelButtons.GetChild(i).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/locked");
            }
        }
        for(int i=0; i<lastUnlocked;i++)//no need to check for existence of the level file
        {
            levelButtons.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text
                = "Level " + (i + 1).ToString() + " - " + PlayerPrefs.GetInt("move_count_" + (i + 1).ToString()) + " Moves";
            levelButtons.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Highest Score: " +
                PlayerPrefs.GetInt("max_score_level_" + (i + 1).ToString()).ToString();
            levelButtons.GetChild(i).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/play");
        }
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("last_unlocked_level", 3);
        PlayerPrefs.Save();
    }
}
