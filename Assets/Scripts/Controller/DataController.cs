using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using System.IO;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;
using UnityEngine.Events;
using System.Linq;
using Spine.Unity;

public class DataController : Singleton<DataController>
{
    [TitleGroup("________Base________")] public string KEY = "KamperTee";
    private string dataPath = "";
    private string FILE_NAME = "data.JSON";
    public GameData gameData;
    [TitleGroup("________Other________")] public List<Sprite> floorSourceSpriteList = new List<Sprite>();
    public List<Sprite> decorationSourceSpriteList = new List<Sprite>();
    public List<SkeletonDataAsset> animationSourceSkeDataAssetList = new List<SkeletonDataAsset>();
    public List<OtherData> otherSourceDataList = new List<OtherData>();
    private List<Sprite> floorSourceSpriteLocalList = new List<Sprite>();
    private List<Sprite> decorationSourceSpriteLocalList = new List<Sprite>();
    private List<SkeletonDataAsset> animationSourceSkeDataAssetLocalList = new List<SkeletonDataAsset>();
    public string DEFAULT_OTHER_DATA_ID = "OtherData_0";
    [TitleGroup("________ErrorNotice________")]
    [SerializeField]
    public Sprite errorSprite;

    public MapData currentMapData
    {
        get => Map.Instance.mapData;
    }

    #region Base

    private void Start()
    {
        dataPath = Path.Combine(Application.persistentDataPath, FILE_NAME);
        Debug.Log("DataPath \n " + dataPath);
        LoadData();
        StartCoroutine(AutoSave());

        IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(20f);
                SaveData();
            }
        }
    }

    public void LoadData()
    {
        Debug.Log("LoadData");
        LoadLocalData();
    }

    public string GetDataJson()
    {
        return File.ReadAllText(dataPath);
    }

    private void LoadLocalData()
    {
        if (File.Exists(dataPath))
        {
            // try
            // {

            string data = GetDataJson();
            // string decrypted = Utils.XOROperator(data, KEY);
            // gameData = JsonUtility.FromJson<GameData>(decrypted);
            gameData = JsonUtility.FromJson<GameData>(data);

            SetOtherId(gameData);
            InitGame();
            Debug.Log("Finished loading local data");
            // }
            // catch (System.Exception e)
            // {
            //     Debug.LogError(e.Message);
            //     ResetData();
            // }
        }
        else
            ResetData();
    }

    private void ResetData()
    {
        InitData();
        InitGame();
        SaveData();
    }

    string floorFolderPath;
    string decorationFolderPath;

    public void InitGame()
    {
        //Camera
        Camera.main.gameObject.transform.position = gameData.cameraPosition;
        Camera.main.orthographicSize = gameData.cameraSize;
        //Create Folder or Get Sprite
        floorFolderPath = GetFolderPath(TileSourceType.FLOOR);
        decorationFolderPath = GetFolderPath(TileSourceType.DECORATION);
        if (!Directory.Exists(floorFolderPath))
        {
            Directory.CreateDirectory(floorFolderPath);
            Directory.CreateDirectory(decorationFolderPath);
            InitGameElement();
        }
        else
        {
            LoadLocalSprites(InitGameElement);
        }
    }

    void DestroyGameElement()
    {
        ObjectPool.Instance.DestroyAll();
        tileSourceSpriteList.Clear();
        TilePalette.Instance.tileSourceList.ForEach(tsItem => Destroy(tsItem.gameObject));
        TilePalette.Instance.tileSourceList.Clear();
        MapPalette.Instance.mapSourceList.ForEach(msItem => Destroy(msItem.gameObject));
        MapPalette.Instance.mapSourceList.Clear();
    }

    void InitGameElement()
    {
        CheckDataBonuses();
        InitTileSourceSpriteList(gameData.tileSourceDataList);
        TilePalette.Instance.Init(gameData.tileSourceDataList);
        //Create UI Map
        MapPalette.Instance.Init(gameData.mapDataList);
        MainCanvas.Instance.InitIndexActive();
    }

    private void CheckDataBonuses()
    {
        bool hasOtherData = gameData.tileSourceDataList.Find(tSDItem => tSDItem.type == TileSourceType.OTHER) != null;
        if (!hasOtherData)
            AddOtherData();
    }

    public void Refresh()
    {
        LoadLocalSprites(InitGameElement);
    }

    public void LoadLocalSprites(UnityAction completed = null)
    {
        //Reset
        decorationSourceSpriteLocalList.Clear();
        floorSourceSpriteLocalList.Clear();
        DestroyGameElement();
        //
        var floorFilePathList = Directory.GetFiles(floorFolderPath);
        var decorationFilePathList = Directory.GetFiles(decorationFolderPath);
        List<string> allFilePath = floorFilePathList.ToList();
        allFilePath.AddRange(decorationFilePathList);
        if (allFilePath.Count != 0)
        {
            int countLoaded = 0;
            foreach (var filePath in allFilePath)
            {
                StartCoroutine(ILoadTileSprite());

                IEnumerator ILoadTileSprite()
                {
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
                    yield return request.SendWebRequest();
                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.Log($"Failed download \n url: <{filePath}>");
                    }
                    else
                    {
                        Texture2D newTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                        var newSprite = Sprite.Create(newTexture,
                            new Rect(0.0f, 0.0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0), 100.0f);
                        newSprite.name = FileBrowserHandle.Instance.GetSpriteIdWithPath(filePath);
                        string tileSourceType = floorFilePathList.Contains(filePath)
                            ? TileSourceType.FLOOR
                            : TileSourceType.DECORATION;
                        switch (tileSourceType)
                        {
                            case TileSourceType.FLOOR:
                                floorSourceSpriteLocalList.Add(newSprite);
                                break;
                            case TileSourceType.DECORATION:
                                decorationSourceSpriteLocalList.Add(newSprite);
                                var a = Map.Instance.gameObject;
                                break;
                        }

                        TileSourceData newTileSourceData = new TileSourceData
                            { id = newSprite.name, type = tileSourceType };
                        if (gameData.tileSourceDataList.Find(tsItem => tsItem.id == newTileSourceData.id) == null)
                            gameData.tileSourceDataList.Add(newTileSourceData);
                        Debug.Log($"Successful download \n url <{filePath}>");
                    }

                    countLoaded++;
                    bool isLoadComplete = countLoaded == allFilePath.Count;
                    if (isLoadComplete)
                        completed?.Invoke();
                }
            }
        }
        else
            completed?.Invoke();
    }

    void InitTileSourceSpriteListElement(TileSourceData tileSource)
    {
        bool isLoadComplete = false;

        void AddSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                tileSourceSpriteList.Add(sprite);
                isLoadComplete = true;
            }
        }

        void AddSkeDataAsset(SkeletonDataAsset skeletonDataAsset)
        {
            if (skeletonDataAsset != null)
            {
                tileSourceSkeDataAssetList.Add(skeletonDataAsset);
                isLoadComplete = true;
            }
        }

        void AddOtherAsset()
        {
            isLoadComplete = true;
        }

        switch (tileSource.type)
        {
            case TileSourceType.FLOOR:
                var sprite1 = floorSourceSpriteList.Find(sprite => sprite.name == tileSource.id);
                AddSprite(sprite1);
                var sprite2 = floorSourceSpriteLocalList.Find(sprite => sprite.name == tileSource.id);
                AddSprite(sprite2);
                break;
            case TileSourceType.DECORATION:
                var sprite3 = decorationSourceSpriteList.Find(sprite => sprite.name == tileSource.id);
                AddSprite(sprite3);
                var sprite4 = decorationSourceSpriteLocalList.Find(sprite => sprite.name == tileSource.id);
                AddSprite(sprite4);
                break;
            case TileSourceType.ANIMATION:
                var skeDataAsset1 =
                    animationSourceSkeDataAssetList.Find(skeletonDataAsset => skeletonDataAsset.name == tileSource.id);
                AddSkeDataAsset(skeDataAsset1);
                var skeDataAsset2 =
                    animationSourceSkeDataAssetLocalList.Find(skeletonDataAsset =>
                        skeletonDataAsset.name == tileSource.id);
                AddSkeDataAsset(skeDataAsset2);
                break;
            case TileSourceType.OTHER:
                AddOtherAsset();
                break;
        }

        if (!isLoadComplete)
        {
            Debug.LogError($"Load Sprite {tileSource.type} {tileSource.id} : False");
            AddSprite(DataController.Instance.errorSprite);
        }
    }

    void InitTileSourceSpriteList(List<TileSourceData> tileSourceDataList)
    {
        tileSourceDataList.ForEach(tileSource => { InitTileSourceSpriteListElement(tileSource); });
    }

    public List<Sprite> tileSourceSpriteList = new List<Sprite>();
    public List<SkeletonDataAsset> tileSourceSkeDataAssetList = new List<SkeletonDataAsset>();

    public void UpdateData()
    {
        var index = gameData.mapDataList.FindIndex(item => item.id == currentMapData.id);
        gameData.mapDataList[index] = currentMapData;
        //Camera
        gameData.cameraPosition = Camera.main.gameObject.transform.position;
        gameData.cameraSize = (int)Camera.main.orthographicSize;
    }

    public void SaveData()
    {
        UpdateData();
        string origin = JsonUtility.ToJson(gameData);
        // string encrypted = Utils.XOROperator(origin, KEY);
        // File.WriteAllText(dataPath, encrypted);
        File.WriteAllText(dataPath, origin);
        Debug.Log("Save data \n " + dataPath);
    }

    public void SaveData(string dataPath)
    {
        string origin = "";
        switch (MapPalette.Instance.exportType)
        {
            case ExportType.currentMap:
                List<MapData> mapDataList = new List<MapData>();
                mapDataList.Add(currentMapData);
                GameData newGameData = new GameData
                    { mapDataList = mapDataList, tileSourceDataList = gameData.tileSourceDataList };
                origin = JsonUtility.ToJson(newGameData);
                break;
            case ExportType.allMap:
                origin = JsonUtility.ToJson(gameData);
                break;
        }

        // string encrypted = Utils.XOROperator(origin, KEY);
        // File.WriteAllText(dataPath, encrypted);
        File.WriteAllText(dataPath, origin);
        Debug.Log("Save data \n " + dataPath);
    }

    public void InitData()
    {
        gameData.cameraPosition = new Vector3(0, 0, -10);
        gameData.cameraSize = 15;
        var newMapData = new MapData
        {
            colunms = 1,
            rows = 1,
            isIndexActive = true
        };
        newMapData.tileDataList.Add(new TileData { });
        floorSourceSpriteList.ForEach(sprite =>
            gameData.tileSourceDataList.Add(new TileSourceData { id = sprite.name, type = TileSourceType.FLOOR }));
        decorationSourceSpriteList.ForEach(sprite =>
            gameData.tileSourceDataList.Add(new TileSourceData { id = sprite.name, type = TileSourceType.DECORATION }));
        animationSourceSkeDataAssetList.ForEach(skeletonDataAsset =>
            gameData.tileSourceDataList.Add(new TileSourceData
                { id = skeletonDataAsset.name, type = TileSourceType.ANIMATION }));
        AddOtherData();
        gameData.mapDataList.Add(newMapData);
    }

    void AddOtherData()
    {
        otherSourceDataList.ForEach(otherSDItem =>
            gameData.tileSourceDataList.Add(new TileSourceData
            {
                id = TileSourceData.GetOtherId(), type = TileSourceType.OTHER,
                otherData = new OtherData
                {
                    moveCost = otherSDItem.moveCost, type = otherSDItem.type, canStand = otherSDItem.canStand,
                    height = otherSDItem.height
                }
            }));
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    public const string FLOOR_FOLDER_NAME = "FloorSpriteList";
    public const string DECORATION_FOLDER_NAME = "DecorationSpriteList";

    public string GetFolderPath(string tileSourceType)
    {
        string folderName = "";
        switch (tileSourceType)
        {
            case TileSourceType.FLOOR:
                folderName = FLOOR_FOLDER_NAME;
                break;
            case TileSourceType.DECORATION:
                folderName = DECORATION_FOLDER_NAME;
                break;
        }

        var folderPath = Path.Combine(Application.persistentDataPath, folderName);
        return folderPath;
    }

    [Button]
    void DeleteData()
    {
        dataPath = Path.Combine(Application.persistentDataPath, FILE_NAME);
        File.Delete(dataPath);
    }

    [Button]
    void DeleteLocalSprite(string tileSourceType, string spriteId)
    {
        var folderPath = GetFolderPath(tileSourceType);
        var dataPath = Path.Combine(folderPath, spriteId + ".png");
        File.Delete(dataPath);
    }

    [Button]
    public void DeleteAllData()
    {
        foreach (var directory in Directory.GetDirectories(Application.persistentDataPath))
        {
            DirectoryInfo data_dir = new DirectoryInfo(directory);
            data_dir.Delete(true);
        }

        foreach (var file in Directory.GetFiles(Application.persistentDataPath))
        {
            FileInfo file_info = new FileInfo(file);
            file_info.Delete();
        }
    }

    #endregion

    public void OnAddTileSourceSprite(Sprite sprite, TileSourceData tileSourceData)
    {
        tileSourceSpriteList.Add(sprite);
        gameData.tileSourceDataList.Add(tileSourceData);
    }

    public void OnRemoveTileSourceSprite(Sprite sprite, TileSourceData tileSourceData)
    {
        if (floorSourceSpriteLocalList.Contains(sprite))
        {
            DeleteLocalSprite(tileSourceData.type, tileSourceData.id);
            floorSourceSpriteLocalList.Remove(sprite);
        }
        else if (decorationSourceSpriteLocalList.Contains(sprite))
        {
            decorationSourceSpriteLocalList.Remove(sprite);
            DeleteLocalSprite(tileSourceData.type, tileSourceData.id);
        }

        tileSourceSpriteList.Remove(sprite);
        gameData.tileSourceDataList.Remove(tileSourceData);
    }

    public void OnAddTileSourceSprite(TileSourceData tileSourceData)
    {
        InitTileSourceSpriteListElement(tileSourceData);
        gameData.tileSourceDataList.Add(tileSourceData);
    }

    public void SetOtherId(GameData gameData)
    {
        gameData.mapDataList.ForEach(mapDataItem =>
        {
            mapDataItem.tileDataList.ForEach(tileDataItem => tileDataItem.SetOtherId());
        });
    }
}

[System.Serializable]
public class GameData
{
    public int currentMapId = 1;
    public List<MapData> mapDataList = new List<MapData>();
    public List<TileSourceData> tileSourceDataList = new List<TileSourceData>();
    public Vector3 cameraPosition = new Vector3(0, 0, -10);
    public int cameraSize = 15;

    public void MergeMapData(List<MapData> mapDataList)
    {
        int maxId = this.mapDataList[this.mapDataList.Count - 1].id;
        mapDataList.ForEach(mapData =>
        {
            maxId++;
            mapData.id = maxId;
            this.mapDataList.Add(mapData);
            MapPalette.Instance.AddAMap(mapData, false);
        });
    }

    public void MergeTileSource(List<TileSourceData> tileSourceDataList)
    {
        tileSourceDataList.ForEach(tileSource =>
        {
            if (this.tileSourceDataList.Find(item => item.id == tileSource.id) == null)
            {
                TilePalette.Instance.AddTileSource(tileSource);
            }
        });
    }

    public bool HasSourceDataWithId(string id)
    {
        foreach (var item in tileSourceDataList)
        {
            if (item.id == id)
            {
                return true;
            }
        }

        return false;
    }
}