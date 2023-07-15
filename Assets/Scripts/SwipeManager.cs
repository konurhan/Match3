using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    public static SwipeManager Instance;
    public Vector2 startTouchPosition;
    public Vector2 endTouchPosition;
    public GameObject grid;
    public int width, height;
    public float cellSize;

    public float swipeTreshold = 60f;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LevelManager.setupEvent += SetSize;
    }
    void Update()
    {
        if(Input.touchCount >0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log("touch began");
            
            startTouchPosition = Input.GetTouch(0).position;
            Debug.Log("touch position is: " + TouchToWorldPosition(startTouchPosition));
        }

        //check if touch started from outside of the grid
        Vector2 startTouchPositionWorld = TouchToWorldPosition(startTouchPosition);
        if (startTouchPositionWorld.x < grid.transform.position.x - width / 2f * cellSize) return;
        if (startTouchPositionWorld.x > grid.transform.position.x + width / 2f * cellSize) return;
        if (startTouchPositionWorld.y < grid.transform.position.y - height / 2f * cellSize) return;
        if (startTouchPositionWorld.y > grid.transform.position.y + height / 2f * cellSize) return;
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Debug.Log("touch ended");
            endTouchPosition = Input.GetTouch(0).position;

            float verticalDifference = Mathf.Abs(endTouchPosition.y - startTouchPosition.y);
            float horizontalDifference = Mathf.Abs(endTouchPosition.x - startTouchPosition.x);
            Debug.Log("vert diff: " + verticalDifference + " hor diff: " + horizontalDifference);

            if (verticalDifference > horizontalDifference)//vertical swap
            {
                if (verticalDifference < swipeTreshold) return;
                Debug.Log("vertical swap");
                Vector2Int cellIndexs = FindCellIndexByTouch(startTouchPosition);
                Debug.Log("cell index -> x: " + cellIndexs.x + " y: " + cellIndexs.y);
                if (endTouchPosition.y - startTouchPosition.y < 0 && cellIndexs.y-1>=0)
                {
                    Debug.Log("swap top to bottom");
                    grid.GetComponent<Grid>().SwapTwoCells(cellIndexs, new Vector2Int(cellIndexs.x, cellIndexs.y-1));
                }
                else if (endTouchPosition.y - startTouchPosition.y > 0 && cellIndexs.y+1 < height)
                {
                    Debug.Log("swap bottom to top");
                    grid.GetComponent<Grid>().SwapTwoCells(cellIndexs, new Vector2Int(cellIndexs.x, cellIndexs.y+1));
                }
            }
            else//horizontal swap
            {
                if (horizontalDifference < swipeTreshold) return;
                Debug.Log("horizontal swap");
                Vector2Int cellIndexs = FindCellIndexByTouch(startTouchPosition);
                Debug.Log("cell index -> x: " + cellIndexs.x + " y: " + cellIndexs.y);
                if (endTouchPosition.x - startTouchPosition.x < 0 && cellIndexs.x-1 >= 0)
                {
                    Debug.Log("swap right to left");
                    grid.GetComponent<Grid>().SwapTwoCells(cellIndexs, new Vector2Int(cellIndexs.x-1, cellIndexs.y));
                }
                else if (endTouchPosition.x - startTouchPosition.x > 0 && cellIndexs.x+1 < width)
                {
                    Debug.Log("swap left to right");
                    grid.GetComponent<Grid>().SwapTwoCells(cellIndexs, new Vector2Int(cellIndexs.x+1, cellIndexs.y));
                }
            }
        }
    }

    public void SetSize()
    {
        width = LevelManager.Instance.width;
        height = LevelManager.Instance.height;
    }

    public Vector2Int FindCellIndexByTouch(Vector2 touchPositionScreen)
    {
        Vector2 touchPosition = TouchToWorldPosition(touchPositionScreen);//(1,0.5)
        Vector2Int cellIndex = new Vector2Int();
        float xOffset;
        float yOffset;
        //Debug.Log("Offset amount for x is: " + width / 2f * cellSize);
        if (touchPosition.x > 0) xOffset = touchPosition.x + width / 2f * cellSize;// = 2.25
        else xOffset = width / 2f * cellSize + touchPosition.x;
        
        if (touchPosition.y > 0) yOffset = touchPosition.y + height / 2f * cellSize;// = 2.25
        else yOffset = height/2f * cellSize + touchPosition.y;

        cellIndex.x = (int)(xOffset / cellSize);
        cellIndex.y = (int)(yOffset / cellSize);
        Debug.Log("touch position is: " + touchPosition);
        Debug.Log("xOffset = "+xOffset+", xIndex = "+ cellIndex.x+ ", yOffset = " +yOffset + ", xIndex = "+ cellIndex.y);
        return cellIndex;
    }

    public Vector2 TouchToWorldPosition(Vector2 touchPosition)
    {
        Vector2 worldPos;
        Vector3 worldPos3D = Camera.main.ScreenToWorldPoint(touchPosition);
        worldPos.x = worldPos3D.x;
        worldPos.y = worldPos3D.y;
        return worldPos;

    }
}
