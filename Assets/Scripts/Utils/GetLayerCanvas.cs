using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GetLayerCanvas : MonoBehaviour
{
    public int minLayer, maxLayer;
    private bool isFirst = true;
    private IEnumerator Start()
    {
        yield return null;
        GetLayer();
    }
    [Button]
    public void GetLayer()
    {
        var canvasList = GetComponentsInChildren<Canvas>();
        for (int i = 0; i < canvasList.Length; i++)
        {
            var layerValue = canvasList[i].sortingOrder;
            if (isFirst)
            {
                minLayer = maxLayer = layerValue;
                isFirst = false;
            }
            else
            {
                if (layerValue > maxLayer)
                {
                    maxLayer = layerValue;
                }
                else if (layerValue < minLayer)
                {
                    minLayer = layerValue;
                }
            }
        }
    }
}
