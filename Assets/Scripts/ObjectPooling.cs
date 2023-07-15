using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;

    public Dictionary<string, List<GameObject>> pooledCubes;
    public Dictionary<string, List<GameObject>> pooledBrokenCubes;
    public int unitPoolingAmount;

    [SerializeField]private List<string> colors;

    private void Awake()
    {
        Instance = this;
        colors = new List<string> {
            "Yellow",
            "Red",
            "Green",
            "Blue"
        };
        pooledCubes = new Dictionary<string, List<GameObject>>();
        pooledBrokenCubes = new Dictionary<string, List<GameObject>>();
        //Invoke("PoolCubes", 2f);
        PoolCubes();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PoolCubes()
    {
        foreach(string color in colors)
        {
            for (int i = 0; i < unitPoolingAmount; i++)
            {
                InstantiateAndAddCube(color);
            }
        }
    }

    public void InstantiateAndAddCube(string color)
    {
        GameObject cube;
        GameObject brokenCube;

        cube = Instantiate(Resources.Load("Models/Prefabs/box-" + color)) as GameObject;
        brokenCube = Instantiate(Resources.Load("Models/Prefabs/broken-pieces-" + color)) as GameObject;

        cube.SetActive(false);
        brokenCube.SetActive(false);

        if (pooledCubes.ContainsKey(color))
        {
            pooledCubes[color].Add(cube);
        }
        else
        {
            pooledCubes.Add(color, new List<GameObject> { cube } );
        }

        if (pooledBrokenCubes.ContainsKey(color))
        {
            pooledBrokenCubes[color].Add(brokenCube);
        }
        else
        {
            pooledBrokenCubes.Add(color, new List<GameObject> { brokenCube });
        }

        cube.transform.SetParent(LevelManager.Instance.CubesParent/*, true*/);
        brokenCube.transform.SetParent(LevelManager.Instance.BrokenCubesParent/*, true*/);
    }

    public GameObject GetPooledCube(string color)
    {
        GameObject cube = null;
        for (int i = 0; i< pooledCubes[color].Count; i++)
        {
            if (!pooledCubes[color][i].activeInHierarchy)
            {
                cube = pooledCubes[color][i];
                break;
            }
        }
        /*if (pooledCubes[color].Count!=0)
        {
            cube = pooledCubes[color][0];
            pooledCubes[color].RemoveAt(0);
        }*/
        if (cube == null)
        {
            InstantiateAndAddCube(color);
            cube = pooledCubes[color][pooledCubes[color].Count - 1];
        }
        cube.SetActive(true);
        return cube;
    }

    public void SetPooledCube(GameObject usedCube)
    {
        string color = usedCube.GetComponent<MeshRenderer>().material.ToString().Replace(" (Instance) (UnityEngine.Material)", "");
        pooledCubes[color].Add(usedCube);
        usedCube.SetActive(false);
    }

    public GameObject GetPooledBrokenCube(string color, Vector3 worldPosition)
    {
        GameObject cube = null;
        for (int i = 0; i < pooledBrokenCubes[color].Count; i++)
        {
            if (!pooledBrokenCubes[color][i].activeInHierarchy)
            {
                cube = pooledBrokenCubes[color][i];
                break;
            }
        }
        /*if (pooledBrokenCubes[color].Count != 0)
        {
            cube = pooledBrokenCubes[color][0];
            pooledBrokenCubes[color].RemoveAt(0);
        }*/
        if (cube == null)
        {
            InstantiateAndAddCube(color);
            cube = pooledBrokenCubes[color][pooledBrokenCubes[color].Count - 1];
        }

        cube.transform.position = worldPosition;
        cube.transform.position += Vector3.back;
        cube.SetActive(true);
        return cube;
    }

    public void SetPooledBrokenCube(GameObject usedCube)
    {
        string color = usedCube.GetComponent<BrokenCube>().color;
        pooledBrokenCubes[color].Add(usedCube);
        usedCube.SetActive(false);
    }
}
