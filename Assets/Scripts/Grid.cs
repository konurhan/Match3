using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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

    public bool setupCompleted = false;

    private void Awake()
    {
        cellSize = 2f;
        Debug.Log("Cell size is: "+cellSize.ToString());
        score = 0;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(LevelManager.Instance.levelInfoIsFetched)
        {
            if (setupCompleted) return;
            SetupGrid();
        }
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

        LevelManager.Instance.canvasLevelTMP.GetComponent<TextMeshProUGUI>().text = "Level: " + levelNumber.ToString();
        LevelManager.Instance.canvasMoveCountTMP.GetComponent<TextMeshProUGUI>().text = "Moves: " + moveCount.ToString();

        SwipeManager.Instance.cellSize = cellSize;
        setWorldPositions();
        InstantiateCells();
    }

    public void setWorldPositions()//setting the world positions of cells
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector3 worldPositionij = new Vector3();
                worldPositionij = gameObject.transform.position + new Vector3((-width/2f)*cellSize + (j+0.5f)*cellSize, (-height / 2f) * cellSize + (i + 0.5f) * cellSize, 0f);//center of the cell in world position
                cellWorldPositions[i,j] = worldPositionij;
            }
        }
    }

    public void InstantiateCells()
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

        moveCount--;
        LevelManager.Instance.canvasMoveCountTMP.GetComponent<TextMeshProUGUI>().text = "Moves: " + moveCount.ToString();
        
        Transform item1StartingPosition = item1.transform;
        Transform item2StartingPosition = item2.transform;

        item1.transform.DOMove(item2StartingPosition.position, 0.2f);
        item2.transform.DOMove(item1StartingPosition.position, 0.2f);

        cells[cellIndex1.y, cellIndex1.x] = item2;
        cells[cellIndex2.y, cellIndex2.x] = item1;

        CheckForMatches(cellIndex1.x, cellIndex1.y, cellIndex2.x, cellIndex2.y);
        BringDownCubesOnTop();

        if (ShouldStop())
        {
            if(score > PlayerPrefs.GetInt("max_score_level_" + levelNumber.ToString()))
            {
                PlayerPrefs.SetInt("max_score_level_" + levelNumber.ToString(), score);
                PlayerPrefs.SetInt("max_score_has_changed_level_" + levelNumber.ToString(), score);
                PlayerPrefs.Save();
            }
            
            SceneManager.LoadScene(0);//loading menu scene
        }
    }

    public void CheckForMatches(int columnIndex1 ,int rowIndex1, int columnIndex2, int rowIndex2)
    {
        bool isHorizontalSwap = true;
        //start by checking for possible matches on the destination index.
        if(columnIndex1 == columnIndex2) isHorizontalSwap=false;

        if (isHorizontalSwap)//check for only 1 row and 2 columns
        {
            CheckRowForMatches(rowIndex1);
            CheckColumnForMatches(columnIndex2);//first we check the column we landed on
            CheckColumnForMatches(columnIndex1);
        }
        else//check for only 1 column and 2 rows
        {
            CheckColumnForMatches(columnIndex1);
            CheckRowForMatches(rowIndex2);
            CheckRowForMatches(rowIndex1);
        }
       
    }

    public void CheckRowForMatches(int rowIndex)
    {
        int consecutiveCount = 0;
        string currentColor = null;
        for(int i=0; i < width; i++)
        {
            //cache the material name
            if (cells[rowIndex, i] == null)
            {
                if (consecutiveCount >= 3)
                {
                    PopRowCubes(rowIndex, i - consecutiveCount, consecutiveCount);
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
                    PopRowCubes(rowIndex, i - consecutiveCount, consecutiveCount); 
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
                    PopRowCubes(rowIndex, i - consecutiveCount + 1, consecutiveCount);
                }
            }
        }
    }

    public void CheckColumnForMatches(int columnIndex)
    {
        //cache the material name
        int consecutiveCount = 0;
        string currentColor = null;
        for (int i = 0; i < height; i++)
        {
            if(cells[i, columnIndex] == null)//if empty slot is found
            {
                if (consecutiveCount >= 3)
                {
                    PopColumnCubes(columnIndex, i - consecutiveCount, consecutiveCount);
                }
                consecutiveCount = 0;
                continue;
            }

            if (consecutiveCount == 0)//if consecutive count is zero
            {
                consecutiveCount = 1;
                //!!!!!!!!!!!!!!!format the following string to get the color name only

                currentColor = cells[i, columnIndex].GetComponent<MeshRenderer>().material.ToString();
                currentColor = currentColor.Replace(" (Instance) (UnityEngine.Material)", "");
                Debug.Log("consecutiveCount == 0: currentColor is " + currentColor);
                continue;
            }

            if (currentColor != cells[i, columnIndex].GetComponent<MeshRenderer>().material.ToString().Replace(" (Instance) (UnityEngine.Material)", ""))//streak has finished
            {
                if (consecutiveCount >= 3)
                {
                    PopColumnCubes(columnIndex, i - consecutiveCount, consecutiveCount);
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
                    PopColumnCubes(columnIndex, i - consecutiveCount + 1, consecutiveCount);
                }
            }
        }
    }


    //patlayan cubeların yerine yukarıdan randomları düşsün, ve üstte kalanlar bir alta kaysın

    public void PopRowCubes(int rowIndex,int startColumnIndex, int size)//play cube explotion animation and set cubes back to the pool
    {
        for (int i = 0; i < size; i++)
        {
            string color = cells[rowIndex, startColumnIndex + i].GetComponent<MeshRenderer>().material.ToString();
            color = color.Replace(" (Instance) (UnityEngine.Material)", "");
            Debug.Log("coordinate of cube to be set is, [" + rowIndex + ", " + startColumnIndex + i + "]");
            ObjectPooling.Instance.SetPooledCube(cells[rowIndex, startColumnIndex + i]);
            cells[rowIndex, startColumnIndex + i] = null;
            GameObject brokenCube = ObjectPooling.Instance.GetPooledBrokenCube(color);
            brokenCube.transform.position = cellWorldPositions[rowIndex, startColumnIndex + i];
            Debug.Log("brokenCube: color is " + color);
        }
    }

    public void PopColumnCubes(int columnIndex, int startRowIndex, int size)//play cube explotion animation and set cubes back to the pool
    {
        for (int i = 0; i < size; i++)
        {
            string color = cells[startRowIndex + i, columnIndex].GetComponent<MeshRenderer>().material.ToString();
            color = color.Replace(" (Instance) (UnityEngine.Material)", "");
            Debug.Log("coordinate of cube to be set is, [" + startRowIndex + i + ", " + columnIndex + "]");
            ObjectPooling.Instance.SetPooledCube(cells[startRowIndex + i, columnIndex]);
            cells[startRowIndex + i, columnIndex] = null;
            GameObject brokenCube = ObjectPooling.Instance.GetPooledBrokenCube(color);
            brokenCube.transform.position = cellWorldPositions[startRowIndex + i, columnIndex];
            Debug.Log("brokenCube: color is " + color);
        }
    }

    public void BringDownCubesOnTop()//goes through each column, finds and caches remainibg boxes, moves them to bottom and fills top with new boxes
    {
        List<int> remainingBoxIndices = new List<int>();
        for (int i = 0; i < width; i++)//for all columns
        {
            for (int j = 0; j < height; j++)//find remaining boxes
            {
                if (cells[j, i] != null) remainingBoxIndices.Add(j);
            }
            int size = remainingBoxIndices.Count;
            for(int k=0; k < size; k++)//for all remaining boxes
            {
                if (k != remainingBoxIndices[k])
                {
                    GameObject cubeToMove = cells[remainingBoxIndices[k], i];
                    //float duration = Mathf.Abs(remainingBoxIndices[k] - k) * 0.2f;
                    cubeToMove.transform.DOMove(cellWorldPositions[k,i], 0.4f);
                    cells[k, i] = cubeToMove;
                    cells[remainingBoxIndices[k], i] = null;
                }
            }
            int amount = height - size;//number of new cubes to get from the pool
            //List<GameObject> movingDown = new List<GameObject>();
            for (int l = 0; l < amount; l++)//cache them to a list and move all of them together
            {
                GameObject newCube = ObjectPooling.Instance.GetPooledCube(GetRandomColor());
                newCube.transform.position = cellWorldPositions[height-1, i] + new Vector3(0, cellSize * (l+1), 0);//put the new cubes above the board
                //movingDown.Add(newCube);
                //float duration = Mathf.Abs(amount - l) * 0.2f;
                newCube.transform.DOMove(cellWorldPositions[size+l, i], 0.4f);
                cells[size + l, i] = newCube;
            }
            /*foreach (GameObject cube in movingDown)
            {
                cube.transform.DOMove(, 0.2f);
            }*/
            remainingBoxIndices.Clear();
        }
    }

    public bool ShouldStop()
    {
        if (moveCount == 0) return true;
        return false;
    }

    public int GetItemTypeCount(string type)
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

    public string GetRandomColor()
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

    
}
