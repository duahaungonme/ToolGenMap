using System.Collections;
using System.Collections.Generic;
using BitBenderGames;
using UnityEngine;
using TMPro;
using Spine.Unity;
using Sirenix.OdinInspector;

public class Tile : MonoBehaviour
{
    public TileData tileData;
    [SerializeField] public SpriteRenderer floorSR, decorationSR;
    [SerializeField] public SkeletonAnimation skeletonAnimation;
    public bool shouldTouch;
    bool isDragging;
    Vector3 mousePos1, mousePos2;
    public TextMeshPro indexText;
    [TitleGroup("___ErrorNotice___")]
    [SerializeField] TextMeshPro errorNoticeFloorText, errorNoticeDecorText, errorNoticeAnimText;
    [SerializeField] GameObject errorNoticeBG;
    [TitleGroup("___OtherData___")]
    [SerializeField] GameObject otherDataGO;
    [SerializeField] TextMeshPro otherText;
    private void Start()
    {
        shouldTouch = true;
    }

    public void Init(TileData tileData, bool isExtendOrCut = false)
    {
        this.tileData = tileData;
        DisableErrorNotice();
        name = $"Tile [{tileData.columnIndex},{tileData.rowIndex}]";
        indexText.text = $"{tileData.columnIndex},{tileData.rowIndex}";
        indexText.gameObject.SetActive(Map.Instance.mapData.isIndexActive);
        if (!isExtendOrCut)
        {
            //Set Floor
            var floorTileSource = TilePalette.Instance.tileSourceList.Find(item => item.tileSourceData.id == tileData.floorId);
            if (floorTileSource == null)
            {
                Debug.LogError($"TileSource {tileData.floorId} is null");
            }
            else
            {
                Sprite sprite = floorTileSource.sprite;
                if (sprite != null)
                    floorSR.sprite = sprite;
                else
                {
                    EnableFloorErrorNotice();
                    // Debug.LogError($"Floor sprite {tileData.floorId} is null");
                }
            }
            //Set Decor
            var decorationTileSource = TilePalette.Instance.tileSourceList.Find(item => item.tileSourceData.id == tileData.decorationId);
            if (decorationTileSource == null)
            {
                Debug.LogError($"TileSource {tileData.decorationId} is null");
            }
            else
            {
                Sprite sprite = decorationTileSource.sprite;
                if (sprite != null)
                    decorationSR.sprite = sprite;
                else
                {
                    EnableDecorErrorNotice();
                    // Debug.LogError($"Decor sprite {tileData.decorationId} is null");
                }
            }
            //Set Anim
            var animTileSource = TilePalette.Instance.tileSourceList.Find(item => item.tileSourceData.id == tileData.animationId);
            if (animTileSource == null)
            {
                Debug.LogError($"TileSource {tileData.animationId} is null");
            }
            else
            {
                SkeletonDataAsset skeletonDataAsset = animTileSource.skeletonDataAsset;
                if (skeletonDataAsset != null)
                    SetAnim(animTileSource.skeletonDataAsset);
                else
                {
                    EnableAnimErrorNotice();
                    // Debug.LogError($"Decor sprite {tileData.animationId} is null");
                }
            }
            //Set Other
            SetOther(tileData);
        }
        else
        {
            switch (TilePalette.Instance.currentState)
            {
                case TileSourceType.FLOOR:
                    //Floor
                    TileSource floorTileSource = TilePalette.Instance.currentFloorTileSource;
                    floorSR.sprite = floorTileSource.sprite;
                    this.tileData.floorId = floorTileSource.tileSourceData.id;
                    SetActiveFloorErrorNotice();
                    break;
                case TileSourceType.DECORATION:
                    //Decor
                    TileSource decorationTileSource = TilePalette.Instance.currentDecorationTileSource;
                    decorationSR.sprite = decorationTileSource.sprite;
                    this.tileData.decorationId = decorationTileSource.tileSourceData.id;
                    SetActiveFloorErrorNotice();
                    break;
                case TileSourceType.ANIMATION:
                    //Anim
                    TileSource animationTileSource = TilePalette.Instance.currentAnimationTileSource;
                    SetAnim(animationTileSource.animationSke.skeletonDataAsset);
                    this.tileData.animationId = animationTileSource.tileSourceData.id;
                    SetActiveAnimErrorNotice();
                    break;
                case TileSourceType.OTHER:
                    var otherTS = TilePalette.Instance.currentOtherTileSource;
                    var otherData = otherTS.tileSourceData.otherData;
                    tileData.moveCost = otherData.moveCost;
                    tileData.canStand = otherData.canStand;
                    tileData.height = otherData.height;
                    tileData.type = otherData.type;
                    tileData.otherId = otherTS.tileSourceData.id;
                    SetOther(tileData);
                    SetActiveAnimErrorNotice();
                    break;
            }
        }
        UpdateOtherGOActive();
    }
    void SetOther(TileData tileData) => otherText.text = $"<color=#FF0000>Id = {TileSource.GetNumberOfOtherId(tileData.otherId)}  </color> \nMove Cost = {tileData.moveCost}  \nType = {tileData.type} \nCan Stand = {tileData.canStand} \nHeight = {tileData.height}";
    public void UpdateOtherGOActive()
    {
        otherDataGO.SetActive(TilePalette.Instance.currentState == TileSourceType.OTHER);
    }
    public void SetAnim(SkeletonDataAsset skeletonDataAsset)
    {
        skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimation.Initialize(true);
    }
    public void OnMouseDown()
    {
        mousePos1 = Camera.main.transform.position;
        isDragging = false;
    }
    public void OnMouseUp()
    {
        mousePos2 = Camera.main.transform.position;
        isDragging = (mousePos1.x != mousePos2.x || mousePos1.y != mousePos2.y || mousePos1.z != mousePos2.z);
        if (!shouldTouch || isDragging)
            return;
        DisableErrorNotice();
        switch (TilePalette.Instance.currentState)
        {
            case TileSourceType.FLOOR:
                var floorTS = TilePalette.Instance.currentFloorTileSource;
                floorSR.sprite = floorTS.floorAndDecorationImage.sprite;
                tileData.floorId = floorTS.tileSourceData.id;
                SetActiveFloorErrorNotice();
                break;
            case TileSourceType.DECORATION:
                var decorTS = TilePalette.Instance.currentDecorationTileSource;
                decorationSR.sprite = decorTS.floorAndDecorationImage.sprite;
                tileData.decorationId = decorTS.tileSourceData.id;
                SetActiveDecorErrorNotice();
                break;
            case TileSourceType.ANIMATION:
                var animTS = TilePalette.Instance.currentAnimationTileSource;
                SetAnim(animTS.animationSke.skeletonDataAsset);
                tileData.animationId = animTS.tileSourceData.id;
                SetActiveAnimErrorNotice();
                break;
            case TileSourceType.OTHER:
                var otherTS = TilePalette.Instance.currentOtherTileSource;
                var otherData = otherTS.tileSourceData.otherData;
                tileData.moveCost = otherData.moveCost;
                tileData.canStand = otherData.canStand;
                tileData.height = otherData.height;
                tileData.type = otherData.type;
                tileData.otherId = otherTS.tileSourceData.id;
                SetOther(tileData);
                SetActiveAnimErrorNotice();
                break;
        }
        SetCurrentTileFocus();
    }
    public void SetCurrentTileFocus()
    {
        Map.Instance.currentTileFocus = this;
        ObjectPool.Instance.DisableGameObjects(Map.Instance.tileFocusPrefab);
        ObjectPool.Instance.GetGameObject(Map.Instance.tileFocusPrefab, transform);
    }
    public void SetSiblingIndex(int value)
    {
        transform.SetSiblingIndex(value);
    }
    void EnableFloorErrorNotice()
    {
        errorNoticeFloorText.gameObject.SetActive(true);
        errorNoticeBG.SetActive(true);
        errorNoticeFloorText.text = $"Missing: floor {tileData.floorId}";
    }
    void SetActiveFloorErrorNotice()
    {
        if (TilePalette.Instance.currentFloorTileSource.isAssetMissing)
            EnableFloorErrorNotice();
    }
    void EnableDecorErrorNotice()
    {
        errorNoticeDecorText.gameObject.SetActive(true);
        errorNoticeBG.SetActive(true);
        errorNoticeDecorText.text = $"Missing: decor {tileData.decorationId}";
    }
    void SetActiveDecorErrorNotice()
    {
        if (TilePalette.Instance.currentDecorationTileSource.isAssetMissing)
            EnableDecorErrorNotice();
    }
    void EnableAnimErrorNotice()
    {
        errorNoticeAnimText.gameObject.SetActive(true);
        errorNoticeBG.SetActive(true);
        errorNoticeAnimText.text = $"Missing: anim {tileData.animationId}";
    }
    void SetActiveAnimErrorNotice()
    {
        if (TilePalette.Instance.currentAnimationTileSource.isAssetMissing)
            EnableAnimErrorNotice();
    }
    void DisableErrorNotice()
    {
        errorNoticeFloorText.gameObject.SetActive(false);
        errorNoticeDecorText.gameObject.SetActive(false);
        errorNoticeAnimText.gameObject.SetActive(false);
        errorNoticeBG.SetActive(false);
    }
}
[System.Serializable]
public class TileData
{
    public float moveCost;
    public float height;
    public string type = "Normal";
    public bool canStand = true;
    public bool isSpawnZone = true;
    public void SetSpawnZone()
    {
        isSpawnZone = (columnIndex >= -4 && columnIndex <= 3) ? false : true;
    }
    public void SetOtherId()
    {
        if (string.IsNullOrEmpty(otherId))
            otherId = DataController.Instance.DEFAULT_OTHER_DATA_ID;
    }
    public int columnIndex, rowIndex;
    public string floorId = "tilemap_empty", decorationId = "UIMask", animationId = "default_SkeletonData", otherId = DataController.Instance.DEFAULT_OTHER_DATA_ID;
    public TileData(int rowIndex = 0, int columnIndex = 0, string floorId = "tilemap_empty", string decorationId = "UIMask", string animationId = "default_SkeletonData")
    {
        this.rowIndex = rowIndex;
        this.columnIndex = columnIndex;
        this.floorId = floorId;
        this.decorationId = decorationId;
        this.animationId = animationId;
    }
    public TileData(TileData tileData)
    {
        this.rowIndex = tileData.rowIndex;
        this.columnIndex = tileData.columnIndex;
        this.floorId = tileData.floorId;
        this.decorationId = tileData.decorationId;
        this.animationId = tileData.animationId;
    }
    public string GetIdWithType(string tileSourceType)
    {
        switch (tileSourceType)
        {
            case TileSourceType.FLOOR:
                return floorId;
            case TileSourceType.DECORATION:
                return decorationId;
            case TileSourceType.ANIMATION:
                return animationId;
        }
        Debug.LogError("Is Null");
        return null;
    }
}
