using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ForceCanvasRatio : MonoBehaviour
{
    [SerializeField] bool isRotation = true;
    [SerializeField] float defaultWidth = 16, defaultHeight = 9;
    [SerializeField] float matchForLongScreen = 1, matchForWideScreen = 0;
    private void Start()
    {
        _Update();
    }
    [Button]
    private void _Update()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        var currentWidth = (float)Screen.width;
        var currentHeight = (float)Screen.height;
        float currentAspect = (currentWidth / currentHeight);
        float defaultAspect = defaultWidth / defaultHeight;
        if (isRotation)
            if (currentAspect <= defaultAspect)
            {
                canvasScaler.matchWidthOrHeight = matchForWideScreen;
            }
            else
            {
                //canvasScaler.referenceResolution = new Vector2(1080, 1920);
                canvasScaler.matchWidthOrHeight = matchForLongScreen;
            }
    }
}
