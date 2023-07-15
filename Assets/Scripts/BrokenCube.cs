using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenCube : MonoBehaviour
{
    //move each piecie on a different trajectory
    //at the and return them to their initial position
    [SerializeField] private List<GameObject> children;//cache in the start method
    [SerializeField] private List<Vector3> initialLocalPositions;//cache in the start method, initial local positions of cube parts
    [SerializeField] private List<Vector3> initialEulerAngles;//cache in the start method
    [SerializeField] public string color;
    [SerializeField] private int particleCount = 4;//shoot first n children of the parent brokenCube object
    [SerializeField] private float gravity = 12.81f;
    [SerializeField] private float passedTime = 0f;
    [SerializeField] private float targetTime = 1.5f;
    [SerializeField] private float verticalVelocity = 10f;
    [SerializeField] private float horizontalVelocity = 2f;
    [SerializeField] private float trajectoryAngle = 60f;
    [SerializeField] private float explosionRegionAngle = 120f;//centered at 270 degrees around y axis
    [SerializeField] private float angularVelocity = 80f;
    [SerializeField] private List<Vector3> normalizedDirections = new List<Vector3>();//horizontal movement directions of cube fragments. Will depend on the explosionRegionAngle

    void Awake()//turn this into awake if particleCount won't change at runtime
    {
        for (int i = 0; i < particleCount; i++)
        {
            children.Add(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < children.Count; i++)
        {
            initialLocalPositions.Add(children[i].transform.localPosition);
            initialEulerAngles.Add(children[i].transform.eulerAngles);
        }

        for (int i = 0; i < particleCount; i++)//only shoot desired number of particles
        {
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = particleCount; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }

        CalculateDirections();
    }

    public void CalculateDirections()
    {
        float startAngle = 180f + (180 - explosionRegionAngle) / 2;
        for(int i=0; i < particleCount; i++)
        {
            float angle = startAngle + i * explosionRegionAngle / (particleCount - 1);
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            normalizedDirections.Add(direction);
        }
    }

    public void Explode()
    {
        passedTime = 0f;
        StartCoroutine(ExplodeBox());
    }

    IEnumerator ExplodeBox()
    {
        while(passedTime < targetTime)
        {
            for (int i = 0; i < particleCount; i++)
            {
                children[i].transform.localPosition += (horizontalVelocity * normalizedDirections[i] + (verticalVelocity - gravity * passedTime)*Vector3.up) * Time.deltaTime;
            }
            for (int i = 0; i < particleCount; i++)
            {
                float x = - angularVelocity * Time.deltaTime;
                float z = angularVelocity * (particleCount /2f -i) * Time.deltaTime / 10;
                children[i].transform.Rotate(x, 0f, z, Space.Self);
            }
            passedTime += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < particleCount; i++)
        {
            children[i].transform.localPosition = initialLocalPositions[i];
            children[i].transform.eulerAngles = initialEulerAngles[i];
        }
        ObjectPooling.Instance.SetPooledBrokenCube(gameObject);
        Debug.Log("ExplodeBox coroutine has finished!!!");
    }
}
