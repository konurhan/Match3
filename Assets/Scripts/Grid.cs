using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;

public class Grid : MonoBehaviour
{
    public int levelNumber;
    public int width;
    public int height;
    public float cellSize;
    public int moveCount;
    public GameObject[,] cells;
    public Vector3[,] cellWorldPositions;
    public char[,] cellTypes;
    public int score;
    public int maxScore;
    public List<int> popAmountByColumn;
    public int[,] explosionMatrix;//pop the cubes in the places of 1s.
    [SerializeField] private List<float> last50Frames;

    public bool isCheckingForMatchesRecursively = false;
    public bool setupCompleted = false;

    private void Awake()
    {
        cellSize = 2f;
        Debug.Log("Cell size is: "+cellSize.ToString());
        score = 0;
        LevelManager.setupEvent += SetupGrid;
    }

    void Start()
    {
        
    }

    void Update()
    {
        CalculateFPS();
    }

    public void CalculateFPS()
    {
        if (last50Frames.Count < 50)
        {
            last50Frames.Add(Time.deltaTime);
            return;
        }

        last50Frames.Add(Time.deltaTime);
        last50Frames.RemoveAt(0);

        float totalTime = 0f;
        for (int i = 0; i < 50; i++)
        {
            totalTime += last50Frames[i];
        }
        LevelManager.Instance.canvasFPSTMP.GetComponent<TextMeshProUGUI>().text = "FPS: " + ((int)(50 / totalTime)).ToString();
    }

    public void SetupGrid()
    {
        width = LevelManager.Instance.width;
        height = LevelManager.Instance.height;
        moveCount = LevelManager.Instance.moveCount;
        levelNumber = LevelManager.Instance.levelNumber;
        maxScore = LevelManager.Instance.maxScore;
        cells = new GameObject[height, width];
        cellWorldPositions = new Vector3[height, width];
        cellTypes = new char[height, width];
        cellTypes = LevelManager.Instance.cellTypes;
        explosionMatrix = new int[height, width];
        ResetExpMatrix();
        LevelManager.Instance.canvasLevelTMP.GetComponent<TextMeshProUGUI>().text = "Level: " + levelNumber.ToString();
        LevelManager.Instance.canvasMoveCountTMP.GetComponent<TextMeshProUGUI>().text = "Moves: " + moveCount.ToString();

        SwipeManager.Instance.cellSize = cellSize;
        setWorldPositions();
        InstantiateCells();
    }

    private void ResetExpMatrix()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0;  j < width;  j++)
            {
                explosionMatrix[i, j] = 0;
            }
        }
    }

    private void setWorldPositions()//setting the world positions of cells
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector3 worldPositionij = new Vector3();
                worldPositionij = gameObject.transform.position + new Vector3((-width/2f)*cellSize + (j+0.5f)*cellSize, (-height / 2f) * cellSize + (i + 0.5f) * cellSize, 0f);//center of the cell in world position
                cellWorldPositions[i,j] = worldPositionij;//filled left to right and buttom-up: [0,0] gives bottom left
            }
        }
    }

    private void InstantiateCells()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject cell = null;
                switch (cellTypes[i,j])
                {
                    case 'y':
                        cell = ObjectPooling.Instance.GetPooledCube("Yellow");
                        break;
                    case 'r':
                        cell = ObjectPooling.Instance.GetPooledCube("Red");
                        break;
                    case 'g':
                        cell = ObjectPooling.Instance.GetPooledCube("Green");
                        break;
                    case 'b':
                        cell = ObjectPooling.Instance.GetPooledCube("Blue");
                        break;
                }
                cell.transform.position = cellWorldPositions[i, j];
                cells[i,j] = cell;
            }
        }
        setupCompleted = true;
    }

    public void SwapTwoCells(Vector2Int cellIndex1, Vector2Int cellIndex2)//first index is gives the cell where touch is started from.
    {
        GameObject item1 = cells[cellIndex1.y, cellIndex1.x];
        GameObject item2 = cells[cellIndex2.y, cellIndex2.x];
        
        if(item1.GetComponent<MeshRenderer>().material.name == item2.GetComponent<MeshRenderer>().material.name) return;//same colored cubes cannot be swaped.
        if (isCheckingForMatchesRecursively) return;

        moveCount--;
        LevelManager.Instance.canvasMoveCountTMP.GetComponent<TextMeshProUGUI>().text = "Moves: " + moveCount.ToString();
        
        Transform item1StartingPosition = item1.transform;
        Transform item2StartingPosition = item2.transform;

        item1.transform.DOMove(item2StartingPosition.position, 0.2f);
        item2.transform.DOMove(item1StartingPosition.position, 0.2f);

        cells[cellIndex1.y, cellIndex1.x] = item2;
        cells[cellIndex2.y, cellIndex2.x] = item1;

        CheckForMatches();
    }

    private void CheckAndStop()
    {
        if (ShouldStop())
        {
            if (score > PlayerPrefs.GetInt("max_score_level_" + levelNumber.ToString()))
            {
                PlayerPrefs.SetInt("max_score_level_" + levelNumber.ToString(), score);
                PlayerPrefs.SetInt("max_score_has_changed_level_" + levelNumber.ToString(), score);
                PlayerPrefs.Save();
            }
            SceneManager.LoadScene(0);//loading menu scene
        }
    }

    private void CheckForMatches()//checks all columns and rows for possible matches and marks them on a matrix
    {
        isCheckingForMatchesRecursively = true;
        
        bool foundAMatch = false;
        for (int i = 0; i < height; i++)
        {
            if(CheckRowForMatches(i)) foundAMatch = true;
        }
        for (int i = 0;i < width; i++)
        {
            if (CheckColumnForMatches(i)) foundAMatch = true;
        }

        if (foundAMatch)
        {
            PopCubesOnMatrix();
            StartCoroutine(DelayNextMatchCheck());
        }
        else
        {
            isCheckingForMatchesRecursively = false;//now the player is allowed to swap again
            CheckAndStop();
        }
    }

    IEnumerator DelayNextMatchCheck()
    {
        yield return new WaitForSeconds(1.2f);
        CheckForMatches();
    }

    private bool CheckRowForMatches(int rowIndex)//call for all columns each time
    {
        bool foundAMatch = false;
        int consecutiveCount = 0;
        string currentColor = null;
        for(int i=0; i < width; i++)
        {
            //cache the material name
            if (cells[rowIndex, i] == null)
            {
                if (consecutiveCount >= 3)
                {
                    MarkRowCubes(rowIndex, i - consecutiveCount, consecutiveCount);
                    foundAMatch = true;
                }
                consecutiveCount = 0;
                continue;
            }

            if (consecutiveCount == 0)
            {
                consecutiveCount = 1;
                currentColor = cells[rowIndex,i].GetComponent<MeshRenderer>().material.ToString();
                currentColor = currentColor.Replace(" (Instance) (UnityEngine.Material)", "");
                continue;
            }
            
            if(currentColor != cells[rowIndex, i].GetComponent<MeshRenderer>().material.ToString().Replace(" (Instance) (UnityEngine.Material)", ""))//streak has finished
            {
                if(consecutiveCount >= 3)
                {
                    MarkRowCubes(rowIndex, i - consecutiveCount, consecutiveCount);
                    foundAMatch = true;
                }
                consecutiveCount = 1;
                currentColor = cells[rowIndex, i].GetComponent<MeshRenderer>().material.ToString();
                currentColor = currentColor.Replace(" (Instance) (UnityEngine.Material)", "");
            }
            else
            {
                consecutiveCount++;
            }

            if(i == width - 1)//covers the case where edge is reached with a streak
            {
                if (consecutiveCount >= 3)
                {
                    MarkRowCubes(rowIndex, i - consecutiveCount + 1, consecutiveCount);
                    foundAMatch = true;
                }
            }
        }
        return foundAMatch;
    }

    private bool CheckColumnForMatches(int columnIndex)//call for all columns after each swap
    {
        bool foundAMatch = false;
        int consecutiveCount = 0;
        string currentColor = null;
        for (int i = 0; i < height; i++)
        {
            if(cells[i, columnIndex] == null)//if empty slot is found
            {
                if (consecutiveCount >= 3)
                {
                    MarkColumnCubes(columnIndex, i - consecutiveCount, consecutiveCount);
                    foundAMatch = true;
                }
                consecutiveCount = 0;
                continue;
            }

            if (consecutiveCount == 0)//if consecutive count is zero
            {
                consecutiveCount = 1;
                currentColor = cells[i, columnIndex].GetComponent<MeshRenderer>().material.ToString();
                currentColor = currentColor.Replace(" (Instance) (UnityEngine.Material)", "");
                Debug.Log("consecutiveCount == 0: currentColor is " + currentColor);
                continue;
            }

            if (currentColor != cells[i, columnIndex].GetComponent<MeshRenderer>().material.ToString().Replace(" (Instance) (UnityEngine.Material)", ""))//streak has finished
            {
                if (consecutiveCount >= 3)
                {
                    MarkColumnCubes(columnIndex, i - consecutiveCount, consecutiveCount);
                    foundAMatch = true;
                }
                consecutiveCount = 1;
                currentColor = cells[i, columnIndex].GetComponent<MeshRenderer>().material.ToString();
                currentColor = currentColor.Replace(" (Instance) (UnityEngine.Material)", "");
            }
            else//streak continues
            {
                consecutiveCount++;
            }

            if (i == height - 1)//covers the case where edge is reached with a streak
            {
                if (consecutiveCount >= 3)
                {
                    MarkColumnCubes(columnIndex, i - consecutiveCount + 1, consecutiveCount);
                    foundAMatch = true;
                }
            }
        }
        return foundAMatch;
    }

    private void PopCubesOnMatrix()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (explosionMatrix[i, j] == 0) continue;
                string color = cells[i, j].GetComponent<MeshRenderer>().material.ToString().Replace(" (Instance) (UnityEngine.Material)", "");
                ObjectPooling.Instance.SetPooledCube(cells[i, j]);
                cells[i, j] = null;
                GameObject brokenCube = ObjectPooling.Instance.GetPooledBrokenCube(color, cellWorldPositions[i,j]);
                brokenCube.GetComponent<BrokenCube>().Explode();
                score++;
                LevelManager.Instance.canvasScoreTMP.GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
            }
        }
        BringDownCubesOnTop();
        ResetExpMatrix();
    }

    private void MarkRowCubes(int rowIndex, int startColumnIndex, int size)
    {
        for (int i = 0; i < size; i++)
        {
            explosionMatrix[rowIndex, startColumnIndex + i] = 1;   
        }
    }

    private void MarkColumnCubes(int columnIndex, int startRowIndex, int size)
    {
        for (int i = 0; i < size; i++)
        {
            explosionMatrix[startRowIndex + i, columnIndex] = 1;
        }
    }

    private void BringDownCubesOnTop()//goes through each column, finds and caches remainibg cubes, moves them to bottom and fills the top with new cubes
    {
        List<int> remainingBoxIndices = new List<int>();
        for (int i = 0; i < width; i++)//for all columns
        {
            for (int j = 0; j < height; j++)//find remaining cubes
            {
                if (cells[j, i] != null) remainingBoxIndices.Add(j);
            }
            int size = remainingBoxIndices.Count;
            for(int k=0; k < size; k++)//for all remaining cubes
            {
                if (k != remainingBoxIndices[k])
                {
                    GameObject cubeToMove = cells[remainingBoxIndices[k], i];
                    cubeToMove.transform.DOMove(cellWorldPositions[k,i], 0.6f);
                    cells[k, i] = cubeToMove;
                    cells[remainingBoxIndices[k], i] = null;
                }
            }
            int amount = height - size;//number of new cubes to get from the pool
            for (int l = 0; l < amount; l++)//cache them to a list and move all of them together
            {
                GameObject newCube = ObjectPooling.Instance.GetPooledCube(GetRandomColor());
                newCube.transform.position = cellWorldPositions[height-1, i] + new Vector3(0, cellSize * (l+1), 0);//put the new cubes above the board
                newCube.transform.DOMove(cellWorldPositions[size+l, i], 0.6f);
                cells[size + l, i] = newCube;
            }
            remainingBoxIndices.Clear();
        }
    }

    private bool ShouldStop()
    {
        if (moveCount == 0) return true;
        return false;
    }

    private int GetItemTypeCount(string type)
    {
        int count = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                string color = cells[i, j].GetComponent<MeshRenderer>().material.ToString();
                color = color.Replace(" (Instance) (UnityEngine.Material)", "");
                if (color == type) count++;
            }
        }
        return count;
    }

    private string GetRandomColor()
    {
        int rand = Random.Range(0, 4);
        string color = null;
        switch (rand)
        {
            case 0:
                color = "Yellow";
                break;
            case 1:
                color = "Red";
                break;
            case 2:
                color = "Green";
                break;
            case 3:
                color = "Blue";
                break;
        }
        return color;
    }

    private void OnDestroy()
    {
        LevelManager.setupEvent -= SetupGrid;
    }
}
