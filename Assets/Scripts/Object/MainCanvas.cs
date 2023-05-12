using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;

public class MainCanvas : Singleton<MainCanvas>
{
    public void OnClickCenterButton()
    {
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    public void OnClickIndexButton()
    {
        bool isIndexActive = DataController.Instance.currentMapData.isIndexActive;
        DataController.Instance.currentMapData.isIndexActive = !isIndexActive;
        DataController.Instance.UpdateData();
        Map.Instance.tileList.ForEach(item => item.indexText.gameObject.SetActive(!isIndexActive));
    }

    public void InitIndexActive()
    {
        bool isIndexActive = DataController.Instance.currentMapData.isIndexActive;
        Map.Instance.tileList.ForEach(item => item.indexText.gameObject.SetActive(isIndexActive));
    }

    [Button]
    public void OnClickUploadButton()
    {
        var str = DataController.Instance.GetDataJson();
        Debug.Log(str);
        APIHandler.Instance.Upload("https://refactor.faraland.moonknightlabs.com/maps/editRoot", str);
    }
}