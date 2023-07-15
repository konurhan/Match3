using System.IO;
using UnityEngine;

public class MenuLevelManager : MonoBehaviour
{
    public static MenuLevelManager Instance;
    public int width, height, moveCount, levelNumber;
    public char[,] cellTypes;
    public string itemTypes;
    public int lastLevel;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPalyerPrefsForLevel(int level)
    {
        ReadLevelInfo(Application.persistentDataPath + "/Levels/RM_A" + level.ToString());
        PlayerPrefs.SetInt("level_number", levelNumber);
        PlayerPrefs.SetInt("grid_width", width);
        PlayerPrefs.SetInt("grid_height", height);
        PlayerPrefs.SetInt("move_count_" + levelNumber.ToString(), moveCount);
        PlayerPrefs.SetString("item_types", itemTypes);
        if (!PlayerPrefs.HasKey("max_score_level_" + level.ToString())) PlayerPrefs.SetInt("max_score_level_" + level.ToString(), 0);
        
        if (!PlayerPrefs.HasKey("max_score_has_changed_level_" + level.ToString())) PlayerPrefs.SetInt("max_score_has_changed_level_" + level.ToString(), 0);
        else PlayerPrefs.SetInt("max_score_has_changed_level_" + levelNumber.ToString(), 0);
        
        PlayerPrefs.SetInt("last_played_level", level);
        PlayerPrefs.Save();
        
        Debug.Log("SetPalyerPrefsForLevel ran");
    }

    public void ReadLevelInfo(string absoluteFilePath)
    {
        string[] lines = File.ReadAllLines(absoluteFilePath);
        lines[0] = lines[0].Replace("level_number: ", "");
        lines[1] = lines[1].Replace("grid_width: ", "");
        lines[2] = lines[2].Replace("grid_height: ", "");
        lines[3] = lines[3].Replace("move_count: ", "");
        lines[4] = lines[4].Replace("grid: ", "");

        levelNumber = int.Parse(lines[0]);
        width = int.Parse(lines[1]);
        height = int.Parse(lines[2]);
        moveCount = int.Parse(lines[3]);
        itemTypes = lines[4];

        int cellTypesIndex = 0;
        cellTypes = new char[height, width];
        for (int i = 0; i < lines[4].Length; i++)
        {
            if (lines[4][i] != ',')
            {
                cellTypes[cellTypesIndex / width, cellTypesIndex % width] = lines[4][i];
                cellTypesIndex++;
            }
        }
    }
}
