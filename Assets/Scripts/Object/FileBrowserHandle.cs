using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.FB.Util;
using UnityEngine.UI;
using Crosstales.FB;
using Crosstales;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;
using Sirenix.OdinInspector;
using Pixelplacement;
using System;

public class FileBrowserHandle : Singleton<FileBrowserHandle>
{
    #region MonoBehaviour methods
    private void Start()
    {
        FileBrowser.Instance.OnOpenFilesComplete += onOpenFilesComplete;
        FileBrowser.Instance.OnOpenFoldersComplete += onOpenFoldersComplete;
        FileBrowser.Instance.OnSaveFileComplete += onSaveFileComplete;
    }

    private void OnDestroy()
    {
        if (FileBrowser.Instance != null)
        {
            FileBrowser.Instance.OnOpenFilesComplete -= onOpenFilesComplete;
            FileBrowser.Instance.OnOpenFoldersComplete -= onOpenFoldersComplete;
            FileBrowser.Instance.OnSaveFileComplete -= onSaveFileComplete;
        }
    }

    #endregion
    string extension;
    #region Public methods
    public void OpenSingleFile(string extension)
    {
        this.extension = extension;
        // ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };
        string[] extensions = { extension };

        // string path = FileBrowser.Instance.OpenSingleFile("Open file", "", "", extensions);
        //string path = FileBrowser.Instance.OpenSingleFile("txt");
        //string path = FileBrowser.Instance.OpenSingleFile();
        string path = FileBrowser.Instance.OpenSingleFile(extension);

        Debug.Log($"OpenSingleFile: '{path}'", this);
    }

    public void OpenFiles()
    {
        //ExtensionFilter[] extensions = { new ExtensionFilter("Binary", "bin"), new ExtensionFilter("Text", "txt", "md"), new ExtensionFilter("C#", "cs") };
        ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg"), new ExtensionFilter("Text", "txt", "md") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };

        string[] paths = FileBrowser.Instance.OpenFiles("Open files", "", "", extensions);
        //string[] paths = FileBrowser.Instance.OpenFiles("txt");
        //string[] paths = FileBrowser.Instance.OpenFiles();

        Debug.Log($"OpenSingleFile: {paths.CTDump()}", this);
    }

    public void OpenSingleFolder()
    {
        //string path = FileBrowser.Instance.OpenSingleFolder("Open folder", testPath);
        string path = FileBrowser.Instance.OpenSingleFolder();

        Debug.Log($"OpenSingleFolder: '{path}'", this);
    }

    public void OpenFolders()
    {
        //string[] paths = FileBrowser.OpenFolders("Open folders", testPath);
        string[] paths = FileBrowser.Instance.OpenFolders();

        Debug.Log($"OpenFolders: {paths.CTDump()}", this);
    }
    [Button]
    public void SaveFile()
    {
        //Add some data for WebGL
        if (Crosstales.FB.Util.Helper.isWebPlatform)
            FileBrowser.Instance.CurrentSaveFileData = System.Text.Encoding.UTF8.GetBytes($"Content created with {Crosstales.FB.Util.Constants.ASSET_NAME}: {Crosstales.FB.Util.Constants.ASSET_PRO_URL}");

        ExtensionFilter[] extensions = { new ExtensionFilter("data", "JSON"), new ExtensionFilter("Text", "JSON", "md"), new ExtensionFilter("C#", "cs") };
        //ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };

        string path = FileBrowser.Instance.SaveFile("Save file", "", "data", extensions);
        //string path = FileBrowser.Instance.SaveFile(null, "txt");
        //string path = FileBrowser.Instance.SaveFile();
        DataController.Instance.SaveData(path);

        Debug.Log($"SaveFile: '{path}'", this);
    }

    public void OpenSingleFileAsync()
    {
        //ExtensionFilter[] extensions = { new ExtensionFilter("Binary", "bin"), new ExtensionFilter("Text", "txt", "md"), new ExtensionFilter("C#", "cs") };
        ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };

        FileBrowser.Instance.OpenSingleFileAsync("Open file", "", "", extensions);
        //FileBrowser.Instance.OpenSingleFileAsync("txt");
        //FileBrowser.Instance.OpenSingleFileAsync();
    }

    public void OpenFilesAsync()
    {
        //ExtensionFilter[] extensions = { new ExtensionFilter("Binary", "bin"), new ExtensionFilter("Text", "txt", "md"), new ExtensionFilter("C#", "cs") };
        ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };

        FileBrowser.Instance.OpenFilesAsync("Open files", "", "", true, extensions);
        //FileBrowser.Instance.OpenFilesAsync(true, "txt");
        //FileBrowser.Instance.OpenFilesAsync();
    }

    public void OpenSingleFolderAsync()
    {
        //FileBrowser.Instance.OpenSingleFolderAsync("Open folder", testPath);
        FileBrowser.Instance.OpenSingleFolderAsync();
    }

    public void OpenFoldersAsync()
    {
        //FileBrowser.Instance.OpenFoldersAsync("Open folders", testPath, true);
        FileBrowser.Instance.OpenFoldersAsync();
    }
    [Button]
    public void SaveFileAsync()
    {
        //Add some data for WebGL
        if (Crosstales.FB.Util.Helper.isWebPlatform)
            FileBrowser.Instance.CurrentSaveFileData = System.Text.Encoding.UTF8.GetBytes($"Content created with {Crosstales.FB.Util.Constants.ASSET_NAME}: {Crosstales.FB.Util.Constants.ASSET_PRO_URL}");

        ExtensionFilter[] extensions = { new ExtensionFilter("Binary", "bin"), new ExtensionFilter("Text", "txt", "md"), new ExtensionFilter("C#", "cs") };
        //ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        //string[] extensions = { "txt", "jpg", "pdf" };
        //string[] extensions = { "txt" };

        FileBrowser.Instance.SaveFileAsync("Save file", "", "MySaveFile", extensions);
        //FileBrowser.Instance.SaveFileAsync(null, "txt");
        //FileBrowser.Instance.SaveFileAsync();

        //FileBrowser.Instance.SaveFileAsync(saveAction);
    }
    #endregion

    public string GetFileNameWithPath(string path)
    {
        string[] strArray = path.Split('\\');
        string fileNamePng = strArray[strArray.Length - 1];
        return fileNamePng;
    }
    public string GetSpriteIdWithPath(string path)
    {
        string[] strArray = path.Split('\\');
        string fileNamePng = strArray[strArray.Length - 1];
        string[] strArray2 = fileNamePng.Split('.');
        return strArray2[0];
    }

    #region File Handle

    void GetTexture(string path)
    {
        var currentState = TilePalette.Instance.currentState;
        StartCoroutine(IGetTexture());
        IEnumerator IGetTexture()
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                //Save image
                var filePath = Path.Combine(DataController.Instance.GetFolderPath(currentState), GetFileNameWithPath(path));
                var spriteId = GetSpriteIdWithPath(path);
                if (!DataController.Instance.gameData.HasSourceDataWithId(spriteId))
                {
                    Sprite tileSourceSprite = null;
                    switch (currentState)
                    {
                        case TileSourceType.FLOOR:
                            tileSourceSprite = DataController.Instance.floorSourceSpriteList.Find(tile => tile.name == spriteId);
                            break;
                        case TileSourceType.DECORATION:
                            tileSourceSprite = DataController.Instance.decorationSourceSpriteList.Find(tile => tile.name == spriteId);
                            break;
                    }
                    if (tileSourceSprite != null)
                    {
                        TilePalette.Instance.OnAddSprite(tileSourceSprite);
                        Debug.Log($"Finished add sprite {spriteId} from data in game");
                    }
                    else if (!File.Exists(filePath))
                    {
                        try
                        {
                            AddSprite(filePath, spriteId, true);
                            Debug.Log($"Finished save and add sprite {spriteId}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else if (File.Exists(filePath))
                    {
                        try
                        {
                            AddSprite(filePath, spriteId, true);
                            Debug.Log($"Finished add sprite {spriteId} from local");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else NoticePanel.Instance.Init($"Load sprite {spriteId} : False");
                }
                else
                {
                    AddSprite(filePath, spriteId, true, false);
                    NoticePanel.Instance.Init($"Finished replace sprite {spriteId}");
                }
            }
            void AddSprite(string _filePath, string _spriteId, bool isSaveFile, bool shouldSpawnTileSource = true)
            {
                Texture2D myTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                byte[] dataBytes = myTexture.EncodeToPNG();
                if (isSaveFile)
                    File.WriteAllBytes(_filePath, dataBytes);
                var newSprite = Sprite.Create(myTexture, new Rect(0.0f, 0.0f, myTexture.width, myTexture.height), new Vector2(0.5f, 0), 100.0f);
                newSprite.name = _spriteId;
                TilePalette.Instance.OnAddSprite(newSprite, shouldSpawnTileSource);
            }
        }
    }
    #endregion

    #region Callbacks

    private void onOpenFilesComplete(bool selected, string singleFile, string[] files)
    {
        Debug.Log($"onOpenFilesComplete: {selected} - '{singleFile}' - {(FileBrowser.Instance.CurrentOpenSingleFileData == null ? "0" : Helper.FormatBytesToHRF(FileBrowser.Instance.CurrentOpenSingleFileData.Length))}", this);
        bool fromTilePalette = extension == "png";
        bool fromMapPalette = extension == "JSON";
        if (fromTilePalette)
            GetTexture(singleFile);
        else if (fromMapPalette)
        {
            string data = File.ReadAllText(singleFile);
            GameData gameData = JsonUtility.FromJson<GameData>(data);

            int GetMaxOtherId()
            {
                string str = String.Empty;
                for (int i = DataController.Instance.gameData.tileSourceDataList.Count - 1; i >= 0; i--)
                {
                    if (DataController.Instance.gameData.tileSourceDataList[i].type == TileSourceType.OTHER)
                    {
                        str = DataController.Instance.gameData.tileSourceDataList[i].id;
                        break;
                    }
                }
                var strArray = str.Split('_');
                return int.Parse(strArray[1]);
            }
            //Replace data
            int maxId = GetMaxOtherId() + 1;
            int count = 0;
            for (int tileSourceIndex = 0; tileSourceIndex < gameData.tileSourceDataList.Count; tileSourceIndex++)
            {
                TileSourceData tempTSD = new TileSourceData { id = gameData.tileSourceDataList[tileSourceIndex].id, type = gameData.tileSourceDataList[tileSourceIndex].type };
                if (tempTSD.type == TileSourceType.OTHER)
                {
                    var strArray = tempTSD.id.Split('_');
                    string idStr = $"{strArray[0]}_{maxId + count}";
                    for (int i = 0; i < gameData.mapDataList.Count; i++)
                    {
                        for (int j = 0; j < gameData.mapDataList[i].tileDataList.Count; j++)
                        {
                            if (gameData.mapDataList[i].tileDataList[j].otherId == tempTSD.id)
                            {
                                gameData.mapDataList[i].tileDataList[j].otherId = idStr;
                            }
                        }
                    }
                    gameData.tileSourceDataList[tileSourceIndex].id = idStr;
                    count++;
                }
            }
            DataController.Instance.SetOtherId(gameData);
            DataController.Instance.gameData.MergeTileSource(gameData.tileSourceDataList);
            DataController.Instance.gameData.MergeMapData(gameData.mapDataList);
            TilePalette.Instance.SetDefault();
        }
    }

    private void onOpenFoldersComplete(bool selected, string singleFolder, string[] folders)
    {
        Debug.Log($"onOpenFoldersComplete: {selected} - '{singleFolder}'", this);
    }

    private void onSaveFileComplete(bool selected, string file)
    {
        Debug.Log($"onSaveFileComplete: {selected} - '{file}' - {(FileBrowser.Instance.CurrentSaveFileData == null ? "0" : Helper.FormatBytesToHRF(FileBrowser.Instance.CurrentSaveFileData.Length))}", this);
    }

    #endregion
}
