using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BitBenderGames;
using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;

public class Map : Singleton<Map>
{
    public MapData mapData;
    [SerializeField] Tile tilePrefab;
    [SerializeField] Transform tileContainer;
    public List<Tile> tileList;
    public float stepX, stepY;
    [SerializeField] public GameObject tileFocusPrefab;
    [SerializeField] public Tile currentTileFocus;
    public MapEditType mapEditType = MapEditType.Normal;
    int tileIndex;
    private void Start()
    {
        mobileTouchCamera = FindObjectOfType<MobileTouchCamera>();
    }
    public void Init(MapData mapData)
    {
        this.mapData = mapData;
        if (mapEditType == MapEditType.Normal)
        {
            clear();
            Dictionary<int, List<TileData>> tileDic = new Dictionary<int, List<TileData>>();
            mapData.tileDataList.ForEach(tile =>
             {
                 List<TileData> value = new List<TileData>();
                 var key = tile.rowIndex;
                 if (tileDic.ContainsKey(key))
                 {
                     tileDic.TryGetValue(key, out value);
                     value.Add(tile);
                     tileDic[key] = value;
                 }
                 else
                 {
                     value.Add(tile);
                     tileDic.Add(key, value);
                 }
             });
            //sort
            Dictionary<int, List<TileData>> sorted = new Dictionary<int, List<TileData>>();
            foreach (var item in tileDic.OrderBy(x => x.Key))
            {
                sorted.Add(item.Key, item.Value);
            }
            tileDic.Clear();
            tileDic = sorted;
            Reset();
            int rowIndex = GetMinRowIndex();
            foreach (var item in tileDic)
            {
                int columnIndex = GetMinColumnIndex();
                item.Value.ForEach(tile =>
                {
                    SpawnTile(tile);
                    columnIndex++;
                });
                rowIndex++;
            }
        }
        else if (mapEditType == MapEditType.Add)
        {
            clear();
            int minRowIndex = (mapData.rows / 2) * -1;
            int minColumnIndex = (mapData.colunms / 2) * -1;
            int maxRowIndex = (mapData.rows / 2) * 1;
            int maxColumnIndex = (mapData.colunms / 2) * 1;
            for (int rowIndex = minRowIndex; rowIndex < maxRowIndex; rowIndex++)
            {
                for (int columnsIndex = minColumnIndex; columnsIndex < maxColumnIndex; columnsIndex++)
                {
                    SpawnTile(new TileData { columnIndex = columnsIndex, rowIndex = rowIndex, floorId = TilePalette.Instance.currentFloorTileSource.tileSourceData.id, decorationId = TilePalette.Instance.currentDecorationTileSource.tileSourceData.id, animationId = TilePalette.Instance.currentAnimationTileSource.tileSourceData.id });
                }
            }
            mapEditType = MapEditType.Normal;
        }
        UpdateSpawnZone();
        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return null;
            tileList[0].SetCurrentTileFocus();
        }
    }
    void clear()
    {
        tileList.ForEach(tile => Destroy(tile.gameObject));
        tileList.Clear();
    }
    public void DeleteMap()
    {
        clear();
        DataController.Instance.gameData.mapDataList.Remove(mapData);
        MapSource item = new MapSource();
        for (int i = MapPalette.Instance.mapSourceList.Count - 1; i >= 0; i--)
        {
            item = MapPalette.Instance.mapSourceList[i];
            if (item.mapData.id == mapData.id)
            {
                Destroy(item.gameObject);
                MapPalette.Instance.mapSourceList.RemoveAt(i);
                int index = MapPalette.Instance.mapSourceList.Count - 1;
                MapPalette.Instance.mapSourceList[index].Select();
                MapPalette.Instance.ScrollToEnd();
                break;
            }
        }
        MapPalette.Instance.UpdateBGMapSource();
    }
    void Reset()
    {
        tileIndex = 0;
    }
    void SpawnTile(TileData tileData, bool isExtendOrCut = false)
    {
        var gO = Instantiate(tilePrefab.gameObject, tileContainer);
        float xBonus = tileData.rowIndex % 2 == 0 ? stepX / 2 : 0;
        float x = (stepX * tileData.columnIndex) + xBonus;
        float y = tileData.rowIndex * stepY;
        gO.transform.position = new Vector3(x, y);
        Tile tile = gO.GetComponent<Tile>();
        tile.Init(tileData, isExtendOrCut);
        tileList.Add(tile);
        mapData.UpdateTileDataList(tileList);
    }
    [Button]
    public void Extend(int extendTypeIndex)
    {
        ExtendType extendType = ExtendType.none;
        extendType = (ExtendType)extendTypeIndex;
        int minRowIndex = GetMinRowIndex();
        int minColumnIndex = GetMinColumnIndex();
        int maxRowIndex = GetMaxRowIndex();
        int maxColumnIndex = GetMaxColumnIndex();
        switch (extendType)
        {
            case ExtendType.Top:
                for (int columnIndex = minColumnIndex; columnIndex < maxColumnIndex + 1; columnIndex++)
                {
                    SpawnTile(new TileData { columnIndex = columnIndex, rowIndex = maxRowIndex + 1 }, true);
                }
                mapData.rows++;
                break;
            case ExtendType.Bottom:
                for (int columnIndex = minColumnIndex; columnIndex < maxColumnIndex + 1; columnIndex++)
                {
                    SpawnTile(new TileData { columnIndex = columnIndex, rowIndex = minRowIndex - 1 }, true);
                }
                mapData.rows++;
                break;
            case ExtendType.Left:
                for (int rowIndex = minRowIndex; rowIndex < maxRowIndex + 1; rowIndex++)
                {
                    SpawnTile(new TileData { columnIndex = minColumnIndex - 1, rowIndex = rowIndex }, true);
                }
                mapData.colunms++;
                break;
            case ExtendType.Right:
                for (int rowIndex = minRowIndex; rowIndex < maxRowIndex + 1; rowIndex++)
                {
                    SpawnTile(new TileData { columnIndex = maxColumnIndex + 1, rowIndex = rowIndex }, true);
                }
                mapData.colunms++;
                break;
        }
        _Update();
        MapPalette.Instance.currentMapSource.UpdateText();
        DataController.Instance.UpdateData();
        UpdateSpawnZone();
    }
    public void UpdateSpawnZone() =>
        DataController.Instance.gameData.mapDataList.Find(mapDataItem => mapDataItem.id == mapData.id).tileDataList.ForEach(tDItem => tDItem.SetSpawnZone());

    [Button]
    public void Cut(int extendTypeIndex)
    {
        ExtendType extendType = ExtendType.none;
        extendType = (ExtendType)extendTypeIndex;
        int minRowIndex = GetMinRowIndex();
        int minColumnIndex = GetMinColumnIndex();
        int maxRowIndex = GetMaxRowIndex();
        int maxColumnIndex = GetMaxColumnIndex();
        switch (extendType)
        {
            case ExtendType.Top:
                if (maxRowIndex != 0)
                    for (int i = tileList.Count - 1; i >= 0; i--)
                    {
                        if (tileList[i].tileData.rowIndex == maxRowIndex)
                        {
                            Destroy(tileList[i].gameObject);
                            tileList.Remove(tileList[i]);
                        }
                    }
                else NoticePanel.Instance.Init("Can't delete tile[0,0]");
                mapData.rows--;
                break;
            case ExtendType.Bottom:
                if (minRowIndex != 0)
                {
                    for (int i = tileList.Count - 1; i >= 0; i--)
                    {
                        if (tileList[i].tileData.rowIndex == minRowIndex)
                        {
                            Destroy(tileList[i].gameObject);
                            tileList.Remove(tileList[i]);
                        }
                    }
                    mapData.rows--;
                }
                else NoticePanel.Instance.Init("Can't delete tile[0,0]");
                break;
            case ExtendType.Left:
                if (minColumnIndex != 0)
                    for (int i = tileList.Count - 1; i >= 0; i--)
                    {
                        if (tileList[i].tileData.columnIndex == minColumnIndex)
                        {
                            Destroy(tileList[i].gameObject);
                            tileList.Remove(tileList[i]);
                        }
                    }
                else NoticePanel.Instance.Init("Can't delete tile[0,0]");
                mapData.colunms--;
                break;
            case ExtendType.Right:
                if (maxColumnIndex != 0)
                {
                    for (int i = tileList.Count - 1; i >= 0; i--)
                    {
                        if (tileList[i].tileData.columnIndex == maxColumnIndex)
                        {
                            Destroy(tileList[i].gameObject);
                            tileList.Remove(tileList[i]);
                        }
                    }
                    mapData.colunms--;
                }
                else NoticePanel.Instance.Init("Can't delete tile[0,0]");
                break;
        }
        mapData.UpdateTileDataList(tileList);
        MapPalette.Instance.currentMapSource.UpdateText();
        DataController.Instance.UpdateData();
    }
    int GetMinRowIndex()
    {
        int value = int.MaxValue;
        mapData.tileDataList.ForEach(tile =>
        {
            if (tile.rowIndex < value)
            {
                value = tile.rowIndex;
            }
        });
        if (value > 0) value = 0;
        return value;
    }
    int GetMinColumnIndex()
    {
        int value = int.MaxValue;
        mapData.tileDataList.ForEach(tile =>
        {
            if (tile.columnIndex < value)
            {
                value = tile.columnIndex;
            }
        });
        if (value > 0) value = 0;
        return value;
    }
    int GetMaxRowIndex()
    {
        int value = int.MinValue;
        mapData.tileDataList.ForEach(tile =>
        {
            if (tile.rowIndex > value)
            {
                value = tile.rowIndex;
            }
        });
        if (value < 0) value = 0;
        return value;
    }
    int GetMaxColumnIndex()
    {
        int value = int.MinValue;
        mapData.tileDataList.ForEach(tile =>
        {
            if (tile.columnIndex > value)
            {
                value = tile.columnIndex;
            }
        });
        if (value < 0) value = 0;
        return value;
    }
    [Button]
    void UpdateData()
    {
        mapData.colunms = (GetMaxColumnIndex() - GetMinColumnIndex()) + 1;
        mapData.rows = (GetMaxRowIndex() - GetMinRowIndex()) + 1;
        DataController.Instance.UpdateData();
    }
    [Button]
    void _Update()
    {
        Dictionary<int, List<Tile>> tileDic = new Dictionary<int, List<Tile>>();
        tileList.ForEach(tile =>
        {
            List<Tile> value = new List<Tile>();
            var key = tile.tileData.rowIndex;
            if (tileDic.ContainsKey(key))
            {
                tileDic.TryGetValue(key, out value);
                value.Add(tile);
                tileDic[key] = value;
            }
            else
            {
                value.Add(tile);
                tileDic.Add(key, value);
            }
        });
        //sort
        Dictionary<int, List<Tile>> sorted = new Dictionary<int, List<Tile>>();
        foreach (var item in tileDic.OrderBy(x => x.Key))
        {
            sorted.Add(item.Key, item.Value);
        }
        tileDic.Clear();
        tileDic = sorted;
        tileDic.Reverse();
        Reset();
        foreach (var item in tileDic)
        {
            item.Value.ForEach(tile =>
            {
                tile.SetSiblingIndex(tileIndex);
                tileIndex++;
            });
        }
    }
    public int GetRows()
    {
        int min = GetMinRowIndex();
        return mapData.rows - min;
    }
    public int GetColumns()
    {
        int min = GetMinColumnIndex();
        return mapData.colunms - min;
    }
    #region Touch Map
    MobileTouchCamera mobileTouchCamera;

    public void OnPointerDown()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        if (mobileTouchCamera != null)
        {
            mobileTouchCamera.isMoveCamera = true;
        }
        Map.Instance.tileList.ForEach(tile => tile.shouldTouch = true);
    }
    Coroutine coroutine;
    public void OnPointerUp()
    {
        coroutine = StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.25f);
            if (mobileTouchCamera != null)
            {
                mobileTouchCamera.isMoveCamera = false;
            }
            Map.Instance.tileList.ForEach(tile => tile.shouldTouch = false);
        }
    }
    #endregion
}
[System.Serializable]
public class MapData
{
    public string name;
    public int id = 1, rows, colunms;
    public List<TileData> tileDataList = new List<TileData>();
    public bool isIndexActive = true;
    public bool HasTileWithFloorId(string floorId)
    {
        return tileDataList.Find(tileData => tileData.floorId == floorId) != null;
    }
    public bool HasTileWithDecorationId(string decorationId)
    {
        return tileDataList.Find(tileData => tileData.decorationId == decorationId) != null;
    }
    public bool HasTileWithOtherId(string otherId)
    {
        return tileDataList.Find(tileData => tileData.otherId == otherId) != null;
    }
    public TileData GetTileDataWithOtherId(string otherId)
    {
        return tileDataList.Find(tileData => tileData.otherId == otherId);
    }
    public void UpdateTileDataList(List<Tile> tileList)
    {
        tileDataList.Clear();
        for (int tileIndex = 0; tileIndex < tileList.Count; tileIndex++)
        {
            if (tileDataList.Count != 0 && tileIndex < tileDataList.Count)
                tileDataList[tileIndex] = tileList[tileIndex].tileData;
            else tileDataList.Add(tileList[tileIndex].tileData);
        }
        // MapPalette.Instance.currentMapSource.mapData = this;
    }
    public MapData(string name = "", int id = 1, int rows = 0, int colunms = 0, bool isIndexActive = true)
    {
        this.name = name;
        this.id = id;
        this.colunms = colunms;
        this.rows = rows;
        this.isIndexActive = isIndexActive;
    }
}
[System.Serializable]
public enum ExtendType
{
    none = 0, Top = 1, Bottom = 2, Left = 3, Right = 4
}
[System.Serializable]
public enum MapEditType
{
    Normal = 0, Add = 1
}