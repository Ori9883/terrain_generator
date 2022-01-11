using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static Utilities instance;
    public List<GameObject> highB = new List<GameObject>();
    public List<GameObject> lowB = new List<GameObject>();


    private void Start()
    {
        instance = this;
    }

    public static T[] ShuffleCoords<T>(T[] _dataArray)
    {
        for (int i = 0; i < _dataArray.Length; i++)
        {
            int random = Random.Range(i, _dataArray.Length);

            T temp = _dataArray[random];
            _dataArray[random] = _dataArray[i];
            _dataArray[i] = temp;
        }

        return _dataArray;
    }

    public GameObject FindBuildingByHeight(float height, float maxHeight , out float yScale)
    {
        int randomNum;
        if (height <= maxHeight / 2)
        {
            randomNum = Random.Range(0, lowB.Count);
            if(height < 1)
            {
                yScale = 1;
            }
            else
            {
                yScale = height;
            }
            return lowB[randomNum];
        }
        else if (height > maxHeight / 2)
        {
            randomNum = Random.Range(0, highB.Count);
            if (height < 2)
            {
                yScale = 1;
            }
            else
            {
                yScale = 1+(height - maxHeight/2);
            }
               
            return highB[randomNum];
        }
        else
        {
            yScale = 0;
            return null;
        }
    }
}
