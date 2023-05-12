using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class MapPalette : Singleton<MapPalette>
{
    [SerializeField] public MapSource mapSourcePrefab, currentMapSource;
    [SerializeField] Transform mapSourceContainer;
    [SerializeField] public List<MapSource> mapSourceList = new List<MapSource>();
    [TitleGroup("___AddButton___")]
    [SerializeField] TMP_InputField nameInput, rowsInput, columnsInput;
    [SerializeField] GameObject createBoard;
    [TitleGroup("___RenameButton___")]
    [SerializeField] TMP_InputField renameInput;
    [SerializeField] GameObject renameBoard;
    [TitleGroup("___ExportButton___")]
    [SerializeField] GameObject exportBoard;

    [TitleGroup("___Other___")]
    public bool initCompleted;
    [SerializeField] ScrollRect scroll;

    public void Init(List<MapData> mapDataList)
    {
        initCompleted = false;
        Map.Instance.mapEditType = MapEditType.Normal;
        mapDataList.ForEach(mapData =>
       {
           AddAMap(mapData, true);
       });
        initCompleted = true;
        mapSourceList.Find(item => item.mapData.id == DataController.Instance.gameData.currentMapId).Select();
        ScrollTo(mapSourceList.FindIndex(item => item.mapData.id == DataController.Instance.gameData.currentMapId));
    }
    public void AddAMap(MapData mapData, bool isSelect)
    {
        var gO = Instantiate(mapSourcePrefab.gameObject, mapSourceContainer);
        MapSource newMapSource = gO.GetComponent<MapSource>();
        newMapSource.Init(mapData, isSelect);
        mapSourceList.Add(newMapSource);
    }
    public void UpdateBGMapSource()
    {
        int index = 0;
        mapSourceList.ForEach(mSItem =>
        {
            mSItem.UpdateBG(index);
            index++;
        });
    }
    #region  Clone Button
    public void OnClickCloneButton()
    {
        var lastMapSource = mapSourceList[mapSourceList.Count - 1];
        MapData newMapData = new MapData
        {
            id = lastMapSource.mapData.id + 1,
            name = currentMapSource.mapData.name + " (clone)",
            colunms = currentMapSource.mapData.colunms,
            rows = currentMapSource.mapData.rows,
            isIndexActive = currentMapSource.mapData.isIndexActive
        };
        currentMapSource.mapData.tileDataList.ForEach(tileData => newMapData.tileDataList.Add(new TileData { rowIndex = tileData.rowIndex, columnIndex = tileData.columnIndex, floorId = tileData.floorId, decorationId = tileData.decorationId, animationId = tileData.animationId, otherId = tileData.otherId, moveCost = tileData.moveCost, isSpawnZone = tileData.isSpawnZone, canStand = tileData.canStand, type = tileData.type, height = tileData.height }));
        DataController.Instance.gameData.mapDataList.Add(newMapData);
        Map.Instance.mapEditType = MapEditType.Normal;
        AddAMap(newMapData, false);
    }
    #endregion

    #region Add Button
    public void OnClickAddButton()
    {
        createBoard.SetActive(true);
    }
    public void OnClickAddCloseButton()
    {
        createBoard.SetActive(false);
    }
    public void OnClickCreateButton()
    {
        try
        {
            if (int.Parse(columnsInput.text) % 2 == 0 && int.Parse(rowsInput.text) % 2 == 0)
            {
                var lastMapSource = mapSourceList[mapSourceList.Count - 1];
                MapData newMapData = new MapData
                {
                    id = mapSourceList[mapSourceList.Count - 1].mapData.id + 1,
                    name = nameInput.text,
                    colunms = int.Parse(columnsInput.text),
                    rows = int.Parse(rowsInput.text),
                    isIndexActive = true
                };
                DataController.Instance.gameData.mapDataList.Add(newMapData);
                Map.Instance.mapEditType = MapEditType.Add;
                AddAMap(newMapData, true);
                MapPalette.Instance.ScrollToEnd();
            }
            else
                NoticePanel.Instance.Init("Create Map: false \n Rows and Columns must be divisible by 2");
        }
        catch (System.Exception)
        {
            NoticePanel.Instance.Init("Create Map: false");
        }
    }
    #endregion

    #region Rename Button
    public void OnClickRenameButton()
    {
        renameBoard.SetActive(true);
    }
    public void OnClickRenameCloseButton()
    {
        renameBoard.SetActive(false);
    }
    public void OnClickChangeButton()
    {
        currentMapSource.mapData.name = renameInput.text;
        currentMapSource.UpdateText();
        DataController.Instance.UpdateData();
    }
    #endregion
    #region Delete Button
    public void OnClickDeleteButton()
    {
        if (DataController.Instance.gameData.mapDataList.Count <= 1)
            NoticePanel.Instance.Init("Don't delete this map");
        else
            NoticePanel.Instance.Init("Are you sure you want to delete this map", () => { Map.Instance.DeleteMap(); NoticePanel.Instance.OnClickCloseButton(); }, () => NoticePanel.Instance.OnClickCloseButton(), () => NoticePanel.Instance.OnClickCloseButton());
    }
    #endregion
    #region Import Button  
    public void OnClickImportButton()
    {
        FileBrowserHandle.Instance.OpenSingleFile("JSON");
    }
    #endregion

    #region Export Button  
    public void OnClickExportButton()
    {
        exportBoard.SetActive(true);
    }
    #endregion
    public void OnClickExportCloseButton()
    {
        exportBoard.SetActive(false);
    }
    public ExportType exportType;
    public void OnClickExportCurrentMapButton()
    {
        exportType = ExportType.currentMap;
        FileBrowserHandle.Instance.SaveFile();
    }
    public void OnClickImportAllMapButton()
    {
        exportType = ExportType.allMap;
        FileBrowserHandle.Instance.SaveFile();
    }

    [Button]
    public void ScrollTo(int indexInList)
    {
        float value = 1f - (float)indexInList / (float)(mapSourceList.Count - 1);
        scroll.verticalNormalizedPosition = value;
    }
    [Button]
    public void ScrollToEnd()
    {
        scroll.verticalNormalizedPosition = 0;
    }
}
[System.Serializable]
public enum ExportType
{
    none, currentMap, allMap
}