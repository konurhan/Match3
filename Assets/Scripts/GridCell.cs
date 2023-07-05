using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell: MonoBehaviour
{
    public Vector2Int cellGridPosition;
    //SpriteRenderer spriteRenderer;
    public char cellType;

    private void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void setCell(char cellType, Vector2Int cellGridPosition)
    {
        this.cellGridPosition = cellGridPosition;
        this.cellType = cellType;
    }






}
