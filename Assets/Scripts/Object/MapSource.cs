using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSource : MonoBehaviour
{
    [SerializeField] public MapData mapData;
    [SerializeField] Color normal1, normal2, selected;
    [SerializeField] Image bG;
    [SerializeField] TextMeshProUGUI text;
    public bool isSelected;
    public void Init(MapData mapData, bool isSelect)
    {
        this.mapData = mapData;
        bG.color = mapData.id % 2 == 0 ? normal2 : normal1;
        if (isSelect)
            Select();
        UpdateText();
    }
    public void Select()
    {
        this.mapData = DataController.Instance.gameData.mapDataList.Find(mapData => mapData.id == this.mapData.id);
        isSelected = true;
        if (MapPalette.Instance.currentMapSource != null)
            MapPalette.Instance.currentMapSource.UnSelect();
        MapPalette.Instance.currentMapSource = this;
        bG.color = selected;
        if (MapPalette.Instance.initCompleted)
        {
            Map.Instance.Init(mapData);
            DataController.Instance.gameData.currentMapId = mapData.id;
        }
    }

    public void UnSelect()
    {
        isSelected = false;
        var indexInList = MapPalette.Instance.mapSourceList.FindIndex(item => item == this);
        UpdateBG(indexInList);
    }
    public void UpdateText()
    {
        text.text = $"Id: {mapData.id} \nName: {mapData.name} \nColums: {mapData.colunms} \nRows: {mapData.rows}";
    }
    public void UpdateBG(int indexInList)
    {
        bG.color = isSelected ? selected : indexInList % 2 == 0 ? normal2 : normal1;
    }
}
