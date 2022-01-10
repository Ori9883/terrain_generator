using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
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
}
