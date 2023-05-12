using System.Collections;
using System.Collections.Generic;
using BitBenderGames;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TilePalette : Singleton<TilePalette>
{
    [SerializeField] public GameObject floorSourceFocusPrefab, decorationSourceFocusPrefab, animationSourceFocusPrefab, otherSourceFocusPrefab, defaultSourceFocusPrefab;

    [SerializeField]
    public TileSource tileSourcePrefab,
        currentFloorTileSource,
        currentDecorationTileSource,
        currentAnimationTileSource, currentOtherTileSource;
    [SerializeField] public Transform floorSourceContainer, decorationSourceContainer, animationSourceContainer, otherSourceContainer, defaultFloorContainer, defaultAnimContainer, defaultDecorContainer, defaultOtherContainer;
    [SerializeField] public List<TileSource> tileSourceList;
    [SerializeField] public List<GameObject> stateButtonList;
    EditType editType;
    [SerializeField] StateMachine scrollViewMachine, editSM;
    [SerializeField] GameObject editState;

    public string currentState;

    #region Tile

    public void Init(List<TileSourceData> tileSourceDataList)
    {
        hasSetDefaultAnim = hasSetDefaultDecor = hasSetDefaultFloor = hasSetDefaultOther = false;
        tileSourceDataList.ForEach(tileSourceData => SpawnTileSource(tileSourceData));
        ChangeState(currentState);
        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return null;
            InitCurrentTileSource();
        }
    }
    public bool hasSetDefaultFloor, hasSetDefaultDecor, hasSetDefaultAnim, hasSetDefaultOther;

    public void SpawnTileSource(TileSourceData tileSourceData, bool shouldSetCurrentTS = false)
    {
        void SetDefault()
        {
            var gO = Instantiate(tileSourcePrefab, GetTransformDefaultWithType(tileSourceData.type));
            TileSource tileSource = gO.GetComponent<TileSource>();
            Init(tileSource);
        }
        if (!hasSetDefaultFloor && tileSourceData.type == TileSourceType.FLOOR)
        {
            SetDefault();
            hasSetDefaultFloor = true;
        }
        else if (!hasSetDefaultDecor && tileSourceData.type == TileSourceType.DECORATION)
        {
            SetDefault();
            hasSetDefaultDecor = true;
        }
        else if (!hasSetDefaultAnim && tileSourceData.type == TileSourceType.ANIMATION)
        {
            SetDefault();
            hasSetDefaultAnim = true;
        }
        else if (!hasSetDefaultOther && tileSourceData.type == TileSourceType.OTHER)
        {
            SetDefault();
            hasSetDefaultOther = true;
        }
        else
        {
            var gO = Instantiate(tileSourcePrefab, GetTransformParentWithType(tileSourceData.type));
            TileSource tileSource = gO.GetComponent<TileSource>();
            Init(tileSource);
        }
        void Init(TileSource tileSource)
        {
            tileSource.Init(tileSourceData);
            tileSourceList.Add(tileSource);
            if (shouldSetCurrentTS)
                SetCurrentTileSource(tileSource);
        }
    }

    Transform GetTransformParentWithType(string tileSourceType)
    {
        switch (tileSourceType)
        {
            case TileSourceType.FLOOR:
                return floorSourceContainer;
            case TileSourceType.DECORATION:
                return decorationSourceContainer;
            case TileSourceType.ANIMATION:
                return animationSourceContainer;
            case TileSourceType.OTHER:
                return otherSourceContainer;
        }
        return null;
    }
    Transform GetTransformDefaultWithType(string tileSourceType)
    {
        switch (tileSourceType)
        {
            case TileSourceType.FLOOR:
                return defaultFloorContainer;
            case TileSourceType.DECORATION:
                return defaultDecorContainer;
            case TileSourceType.ANIMATION:
                return defaultAnimContainer;
            case TileSourceType.OTHER:
                return defaultOtherContainer;
        }
        return null;
    }

    public void OnClickAddButton()
    {
        editType = EditType.Add;
        FileBrowserHandle.Instance.OpenSingleFile("png");
    }

    public void OnClickDetailButton()
    {
        TileSource tileSource = new TileSource();
        switch (currentState)
        {
            case TileSourceType.FLOOR:
                tileSource = currentFloorTileSource;
                break;
            case TileSourceType.DECORATION:
                tileSource = currentDecorationTileSource;
                break;
            case TileSourceType.ANIMATION:
                tileSource = currentAnimationTileSource;
                break;
        }

        NoticePanel.Instance.Init($"Id = {tileSource.tileSourceData.id}\n type = {tileSource.tileSourceData.type}");
    }

    public void OnClickRemoveButton()
    {
        editType = EditType.Remove;
        switch (currentState)
        {
            case TileSourceType.FLOOR:
                OnRemove(currentFloorTileSource, DataController.Instance.floorSourceSpriteList[0]);
                break;
            case TileSourceType.DECORATION:
                OnRemove(currentDecorationTileSource, DataController.Instance.decorationSourceSpriteList[0]);
                break;
        }

        void OnRemove(TileSource currentTileSource, Sprite defaultTileSourceSprite)
        {
            bool isDefaultTile = currentTileSource.sprite == defaultTileSourceSprite;
            bool isTileOnMap = false;
            switch (currentState)
            {
                case TileSourceType.FLOOR:
                    DataController.Instance.gameData.mapDataList.ForEach(mapDataItem =>
                        isTileOnMap |= mapDataItem.HasTileWithFloorId(currentTileSource.tileSourceData.id));
                    break;
                case TileSourceType.DECORATION:
                    DataController.Instance.gameData.mapDataList.ForEach(mapDataItem =>
                        isTileOnMap |= mapDataItem.HasTileWithDecorationId(currentTileSource.tileSourceData.id));
                    break;
            }

            if (!isDefaultTile && !isTileOnMap)
            {
                DataController.Instance.OnRemoveTileSourceSprite(currentTileSource.sprite,
                    currentTileSource.tileSourceData);
                tileSourceList.Remove(currentTileSource);
                Destroy(currentTileSource.gameObject);
                SetCurrentTileSource(currentTileSource.tileSourceData.type);
            }
            else
            {
                if (isDefaultTile)
                    NoticePanel.Instance.Init("Can't delete default tile_source");
                else if (isTileOnMap)
                {
                    TileData tileData = new TileData();
                    int mapId = 0;
                    switch (currentState)
                    {
                        case TileSourceType.FLOOR:
                            foreach (var item in DataController.Instance.gameData.mapDataList)
                            {
                                var _tileData = item.tileDataList.Find(tileDataItem =>
                                    tileDataItem.floorId == currentTileSource.tileSourceData.id);
                                if (_tileData != null)
                                {
                                    tileData = _tileData;
                                    mapId = item.id;
                                    break;
                                }
                            }

                            break;
                        case TileSourceType.DECORATION:
                            foreach (var item in DataController.Instance.gameData.mapDataList)
                            {
                                var _tileData = item.tileDataList.Find(tileDataItem =>
                                    tileDataItem.decorationId == currentTileSource.tileSourceData.id);
                                if (_tileData != null)
                                {
                                    tileData = _tileData;
                                    mapId = item.id;
                                    break;
                                }
                            }

                            break;
                    }

                    NoticePanel.Instance.Init(
                        $"Can't delete tile_source on map[id = {mapId} ,rowIndex = {tileData.rowIndex}, columnIndex ={tileData.columnIndex}]");
                }
            }
        }
    }

    public void SetCurrentTileSource(string tileSourceType)
    {
        var type = tileSourceType;
        switch (type)
        {
            case TileSourceType.FLOOR:
                currentFloorTileSource =
                    tileSourceList.Find(tileSource => tileSource.tileSourceData.type == tileSourceType);
                currentFloorTileSource.OnClickButton(false);
                break;
            case TileSourceType.DECORATION:
                currentDecorationTileSource =
                    tileSourceList.Find(tileSource => tileSource.tileSourceData.type == tileSourceType);
                currentDecorationTileSource.OnClickButton(false);
                break;
        }
    }

    public void SetCurrentTileSource(TileSource tileSource)
    {
        var type = tileSource.tileSourceData.type;
        switch (type)
        {
            case TileSourceType.FLOOR:
                currentFloorTileSource = tileSource;
                currentFloorTileSource.OnClickButton(false);
                break;
            case TileSourceType.DECORATION:
                currentDecorationTileSource = tileSource;
                currentDecorationTileSource.OnClickButton(false);
                break;
            case TileSourceType.ANIMATION:
                currentAnimationTileSource = tileSource;
                currentAnimationTileSource.OnClickButton(false);
                break;
            case TileSourceType.OTHER:
                currentOtherTileSource = tileSource;
                currentOtherTileSource.OnClickButton(false);
                break;
        }
    }

    #endregion

    #region State

    bool isInit;

    public void ChangeState(string stateName)
    {
        currentState = stateName;
        stateButtonList.ForEach(item =>
        {
            bool isSelected = item.name == stateName;
            var color = item.GetComponent<Image>().color;
            item.GetComponent<Image>().color = new Color(color.r, color.g, color.b, isSelected ? 0 : 1);
            item.GetComponent<Button>().interactable = !isSelected;
        });
        if (!isInit)
        {
            switch (stateName)
            {
                case TileSourceType.FLOOR:
                    SetCurrentTileSource(currentFloorTileSource != null
                        ? currentFloorTileSource
                        : tileSourceList.Find(tileSource => tileSource.tileSourceData.type == TileSourceType.FLOOR));
                    break;
                case TileSourceType.DECORATION:
                    SetCurrentTileSource(currentDecorationTileSource != null
                        ? currentDecorationTileSource
                        : tileSourceList.Find(tileSource =>
                            tileSource.tileSourceData.type == TileSourceType.DECORATION));
                    break;
                case TileSourceType.ANIMATION:
                    SetCurrentTileSource(currentAnimationTileSource != null
                        ? currentAnimationTileSource
                        : tileSourceList.Find(tileSource =>
                            tileSource.tileSourceData.type == TileSourceType.ANIMATION));
                    break;
                case TileSourceType.OTHER:
                    SetCurrentTileSource(currentOtherTileSource != null
                        ? currentOtherTileSource
                        : tileSourceList.Find(tileSource =>
                            tileSource.tileSourceData.type == TileSourceType.OTHER));
                    break;
            }

            isInit = true;
        }
        else
        {
            scrollViewMachine.ChangeState(stateName);
            switch (stateName)
            {
                case TileSourceType.FLOOR:
                case TileSourceType.DECORATION:
                case TileSourceType.OTHER:
                    editState.SetActive(true);
                    editSM.ChangeState(0);
                    break;
                case TileSourceType.ANIMATION:
                    editSM.ChangeState(1);
                    editState.SetActive(false);
                    break;
            }
            switch (stateName)
            {
                case TileSourceType.FLOOR:
                case TileSourceType.DECORATION:
                case TileSourceType.ANIMATION:
                    editSM.ChangeState(0);
                    break;
                case TileSourceType.OTHER:
                    editSM.ChangeState(1);
                    break;
            }
        }
        Map.Instance.tileList.ForEach(tileItem => tileItem.UpdateOtherGOActive());
    }

    void InitCurrentTileSource()
    {
        SetCurrentTileSource(currentFloorTileSource != null
            ? currentFloorTileSource
            : tileSourceList.Find(tileSource => tileSource.tileSourceData.type == TileSourceType.FLOOR));
        SetCurrentTileSource(currentDecorationTileSource != null
            ? currentDecorationTileSource
            : tileSourceList.Find(tileSource => tileSource.tileSourceData.type == TileSourceType.DECORATION));
        SetCurrentTileSource(currentAnimationTileSource != null
            ? currentAnimationTileSource
            : tileSourceList.Find(tileSource => tileSource.tileSourceData.type == TileSourceType.ANIMATION));
        SetCurrentTileSource(currentOtherTileSource != null
            ? currentOtherTileSource
            : tileSourceList.Find(tileSource => tileSource.tileSourceData.type == TileSourceType.OTHER));
    }

    public void OnAddSprite(Sprite newSprite, bool shouldSpawnTileSource = true)
    {
        TileSourceData newTileSourceData = new TileSourceData { id = newSprite.name, type = currentState };
        if (shouldSpawnTileSource)
        {
            DataController.Instance.OnAddTileSourceSprite(newSprite, newTileSourceData);
            SpawnTileSource(newTileSourceData, true);
        }
        else
        {
            var tileSource = tileSourceList.Find(tSItem => tSItem.tileSourceData.id == newTileSourceData.id);
            if (tileSource != null)
            {
                tileSource.SetSprite(newSprite);
                tileSource.HideError();
            }

            Map.Instance.tileList.ForEach(tileItem =>
            {
                if (tileItem.tileData.GetIdWithType(newTileSourceData.type) == newTileSourceData.id)
                    tileItem.Init(tileItem.tileData);
            });
        }
    }

    public void AddTileSource(TileSourceData tileSourceData)
    {
        TileSourceData newTileSourceData = new TileSourceData { id = tileSourceData.id, type = tileSourceData.type, otherData = new OtherData { moveCost = tileSourceData.otherData.moveCost, canStand = tileSourceData.otherData.canStand, type = tileSourceData.otherData.type, height = tileSourceData.otherData.height } };
        DataController.Instance.OnAddTileSourceSprite(newTileSourceData);
        SpawnTileSource(newTileSourceData, tileSourceData.type != TileSourceType.OTHER);
    }

    #endregion

    public void SetDefault()
    {
        currentFloorTileSource = tileSourceList.Find(item => item.tileSourceData.type == TileSourceType.FLOOR);
        currentDecorationTileSource =
            tileSourceList.Find(item => item.tileSourceData.type == TileSourceType.DECORATION);
        currentAnimationTileSource = tileSourceList.Find(item => item.tileSourceData.type == TileSourceType.ANIMATION);
        currentOtherTileSource = tileSourceList.Find(item => item.tileSourceData.type == TileSourceType.OTHER);
        currentFloorTileSource.OnClickButton();
        currentDecorationTileSource.OnClickButton();
        currentAnimationTileSource.OnClickButton();
        currentOtherTileSource.OnClickButton();
    }

    [TitleGroup("Path Button")]
    [SerializeField]
    GameObject pathBoard;

    [SerializeField] TMP_InputField pathOutput;

    public void OnClickPathButton()
    {
        pathBoard.SetActive(true);
        string path;
        path = DataController.Instance.GetFolderPath(currentState);
        pathOutput.text = path;
    }

    public void OnClickPathCloseButton()
    {
        pathBoard.SetActive(false);
    }
    [TitleGroup("___Other Data___")]
    #region Other Data
    #region Add Button
    [SerializeField] TMP_InputField moveCostInput, typeInput, canStandInput, heightInput;
    [SerializeField] GameObject createBoard;

    public void OnClickOtherAddButton()
    {
        createBoard.SetActive(true);
    }
    public void OnClickOtherAddCreateButton()
    {
        try
        {
            if (canStandInput.text == "true" || canStandInput.text == "false")
            {
                TileSourceData newTileSourceData = new TileSourceData { id = TileSourceData.GetOtherId(), type = TileSourceType.OTHER, otherData = new OtherData { moveCost = int.Parse(moveCostInput.text), type = typeInput.text, canStand = bool.Parse(canStandInput.text), height = float.Parse(heightInput.text) } };
                DataController.Instance.gameData.tileSourceDataList.Add(newTileSourceData);
                SpawnTileSource(newTileSourceData);
            }
            else
                NoticePanel.Instance.Init("Create TileSource: false \n Request\n Can Stand = true / false 2\n Move Cost = 0,1,2,3.. \n type =  'Normal' | 'DeadPoint' | 'Grass'");
        }
        catch (System.Exception)
        {
            NoticePanel.Instance.Init("Create TileSource: false \n Request\n Can Stand = true / false 2\n Move Cost = 0,1,2,3.. \n type =  'Normal' | 'DeadPoint' | 'Grass'");
        }
    }
    public void OnClickOtherAddCloseButton()
    {
        createBoard.SetActive(false);
    }
    #endregion
    #region Clone Button
    public void OnClickOtherCloneButton()
    {
        var curOtherTSD = TilePalette.Instance.currentOtherTileSource.tileSourceData;
        TileSourceData newTileSourceData = new TileSourceData { id = TileSourceData.GetOtherId(), type = curOtherTSD.type, otherData = new OtherData { moveCost = curOtherTSD.otherData.moveCost, type = curOtherTSD.otherData.type, canStand = curOtherTSD.otherData.canStand, height = curOtherTSD.otherData.height } };
        DataController.Instance.gameData.tileSourceDataList.Add(newTileSourceData);
        SpawnTileSource(newTileSourceData);
    }
    #endregion
    #region Delete Button
    public void OnClickOtherDeleteButton()
    {
        if (currentOtherTileSource.tileSourceData.id != DataController.Instance.DEFAULT_OTHER_DATA_ID)
        {
            int mapId = 0;
            TileData tileData = null;
            bool isTileOnMap = false;
            DataController.Instance.gameData.mapDataList.ForEach(mapDataItem =>
            {
                isTileOnMap |= mapDataItem.HasTileWithOtherId(currentOtherTileSource.tileSourceData.id);
                if (isTileOnMap && tileData == null)
                {
                    tileData = mapDataItem.GetTileDataWithOtherId(currentOtherTileSource.tileSourceData.id);
                    mapId = mapDataItem.id;
                }
            });

            if (!isTileOnMap)
                for (int i = tileSourceList.Count - 1; i >= 0; i--)
                {
                    if (tileSourceList[i].tileSourceData.id == currentOtherTileSource.tileSourceData.id)
                    {
                        Destroy(tileSourceList[i].gameObject);
                        tileSourceList.RemoveAt(i);
                        foreach (var item in DataController.Instance.gameData.tileSourceDataList)
                        {
                            if (item.id == currentOtherTileSource.tileSourceData.id)
                            {
                                DataController.Instance.gameData.tileSourceDataList.Remove(item);
                                break;
                            }
                        }
                        currentOtherTileSource = tileSourceList.Find(tSItem => tSItem.tileSourceData.type == TileSourceType.OTHER);
                        currentOtherTileSource.OnClickButton();
                        break;
                    }
                }
            else
                NoticePanel.Instance.Init(
                    $"Can't delete tile_source on map[id = {mapId} ,rowIndex = {tileData.rowIndex}, columnIndex ={tileData.columnIndex}]");
        }
        else
            NoticePanel.Instance.Init("Don't delete this default tile");
    }
    #endregion
    #region edit Button
    [SerializeField] TMP_InputField editMoveCostInput, editTypeInput, editCanStandInput, editHeightInput;
    [SerializeField] GameObject editBoard;

    public void OnClickOtherEditButton()
    {
        editBoard.SetActive(true);
    }
    public void OnClickOtherEditConfirmButton()
    {
        try
        {
            if (editCanStandInput.text == "true" || editCanStandInput.text == "false")
            {
                TileSourceData tileSourceData = currentOtherTileSource.tileSourceData;
                tileSourceData.otherData.moveCost = float.Parse(editMoveCostInput.text);
                tileSourceData.otherData.type = editTypeInput.text;
                tileSourceData.otherData.canStand = bool.Parse(editCanStandInput.text);
                tileSourceData.otherData.height = float.Parse(editHeightInput.text);
                currentOtherTileSource.SetOtherData(tileSourceData);
                for (int i = 0; i < DataController.Instance.gameData.mapDataList.Count; i++)
                {
                    for (int j = 0; j < DataController.Instance.gameData.mapDataList[i].tileDataList.Count; j++)
                    {
                        if (DataController.Instance.gameData.mapDataList[i].tileDataList[j].otherId == tileSourceData.id)
                        {
                            DataController.Instance.gameData.mapDataList[i].tileDataList[j].canStand = tileSourceData.otherData.canStand;
                            DataController.Instance.gameData.mapDataList[i].tileDataList[j].type = tileSourceData.otherData.type;
                            DataController.Instance.gameData.mapDataList[i].tileDataList[j].moveCost = tileSourceData.otherData.moveCost;
                            DataController.Instance.gameData.mapDataList[i].tileDataList[j].height = tileSourceData.otherData.height;
                        }
                    }
                }
                DataController.Instance.Refresh();
            }
            else
                NoticePanel.Instance.Init("Create TileSource: false \n Request\n Can Stand = true / false 2\n Move Cost = 0,1,2,3.. \n type =  'Normal' | 'DeadPoint' | 'Grass'");
        }
        catch (System.Exception)
        {
            NoticePanel.Instance.Init("Create TileSource: false \n Request\n Can Stand = true / false 2\n Move Cost = 0,1,2,3.. \n type =  'Normal' | 'DeadPoint' | 'Grass'");
        }
    }
    public void OnClickOtherEditCloseButton()
    {
        editBoard.SetActive(false);
    }
    #endregion
    #endregion
}
[System.Serializable]
public enum EditType
{
    none,
    Add,
    Remove
}