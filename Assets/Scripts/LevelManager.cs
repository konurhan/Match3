using UnityEngine;

public class LevelManager: MonoBehaviour
{
    public static LevelManager Instance;
    public int width, height, moveCount, levelNumber;
    public char[,] cellTypes;
    public string itemTypes;
    public int maxScore;

    public Transform canvasLevelTMP;
    public Transform canvasMoveCountTMP;
    public Transform canvasScoreTMP;
    public Transform canvasFPSTMP;

    public Transform CubesParent;
    public Transform BrokenCubesParent;

    public bool levelInfoIsFetched = false;

    public delegate void Setup();
    public static event Setup setupEvent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadFromPlayerPrefs();
        setupEvent.Invoke();
        levelInfoIsFetched = true;
    }

    public void LoadFromPlayerPrefs()
    {
        levelNumber = PlayerPrefs.GetInt("level_number");
        width = PlayerPrefs.GetInt("grid_width");
        height = PlayerPrefs.GetInt("grid_height");
        moveCount = PlayerPrefs.GetInt("move_count_" + levelNumber.ToString());
        itemTypes = PlayerPrefs.GetString("item_types");
        maxScore = PlayerPrefs.GetInt("max_score_level_" + levelNumber.ToString());

        int cellTypesIndex = 0;
        cellTypes = new char[height, width];
        for (int i = 0; i < itemTypes.Length; i++)
        {
            if (itemTypes[i] != ',')
            {
                cellTypes[cellTypesIndex / width, cellTypesIndex % width] = itemTypes[i];
                cellTypesIndex++;
            }
        }
    }
}
