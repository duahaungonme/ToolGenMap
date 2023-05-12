using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Sirenix.OdinInspector;
using TMPro;

public class TileSource : MonoBehaviour
{
    [SerializeField] public TileSourceData tileSourceData;
    [SerializeField] public Image floorAndDecorationImage;
    [HideInInspector] public Sprite sprite;
    [SerializeField] public SkeletonGraphic animationSke;
    [HideInInspector] public SkeletonDataAsset skeletonDataAsset;
    [SerializeField] Transform focusContainer;
    GameObject focusGO;
    [TitleGroup("___OtherData___")]
    [SerializeField] public GameObject otherGO;
    [SerializeField] public TextMeshProUGUI otherText;
    [TitleGroup("___ErrorNotice___")]
    [SerializeField] GameObject errorNotice;
    [SerializeField] TextMeshProUGUI errorNoticeText;
    [SerializeField] Button button;
    public bool isAssetMissing;

    public void Init(TileSourceData tileSourceData)
    {
        this.tileSourceData = tileSourceData;
        errorNotice.SetActive(false);
        isAssetMissing = false;
        // button.interactable = true;
        animationSke.gameObject.SetActive(false);
        floorAndDecorationImage.gameObject.SetActive(false);
        otherGO.SetActive(false);
        switch (tileSourceData.type)
        {
            case TileSourceType.FLOOR:
            case TileSourceType.DECORATION:
                floorAndDecorationImage.gameObject.SetActive(true);
                SetSprite(DataController.Instance.tileSourceSpriteList.Find(item => item.name == tileSourceData.id));
                break;
            case TileSourceType.ANIMATION:
                animationSke.gameObject.SetActive(true);
                SetAnimData(DataController.Instance.tileSourceSkeDataAssetList.Find(item => item.name == tileSourceData.id));
                break;
            case TileSourceType.OTHER:
                otherGO.SetActive(true);
                SetOtherData(tileSourceData);
                break;
        }
    }
    public void ShowError()
    {
        isAssetMissing = true;
        errorNotice.SetActive(true);
        errorNoticeText.text = $"Missing: {tileSourceData.id}";
        // button.interactable = false;
    }
    public void HideError()
    {
        isAssetMissing = false;
        errorNotice.SetActive(false);
        // button.interactable = true;
    }
    public void SetSprite(Sprite sprite)
    {
        this.sprite = sprite;
        if (sprite != null)
            floorAndDecorationImage.sprite = sprite;
        else ShowError();
    }
    public void SetAnimData(SkeletonDataAsset skeletonDataAsset)
    {
        this.skeletonDataAsset = skeletonDataAsset;
        if (skeletonDataAsset != null)
        {
            animationSke.skeletonDataAsset = skeletonDataAsset;
            animationSke.Initialize(true);
        }
        else ShowError();
    }
    public void SetOtherData(TileSourceData tileSourceData)
    {
        var otherData = tileSourceData.otherData;
        if (otherData != null)
        {
            otherText.text = $"Id = {GetNumberOfOtherId(tileSourceData.id)} \nMove Cost = {otherData.moveCost}  \nType = {otherData.type} \nCan Stand = {otherData.canStand} \nHeight = {otherData.height}";
        }
    }
    public static string GetNumberOfOtherId(string otherId)
    {
        var strArray = otherId.Split('_');
        return strArray[1];
    }
    public void OnClickButton(bool isSetCurrentTile = true)
    {
        switch (tileSourceData.type)
        {
            case TileSourceType.FLOOR:
                focusGO = TilePalette.Instance.floorSourceFocusPrefab;
                if (isSetCurrentTile)
                    TilePalette.Instance.currentFloorTileSource = this;
                break;
            case TileSourceType.DECORATION:
                focusGO = TilePalette.Instance.decorationSourceFocusPrefab;
                if (isSetCurrentTile)
                    TilePalette.Instance.currentDecorationTileSource = this;
                break;
            case TileSourceType.ANIMATION:
                focusGO = TilePalette.Instance.animationSourceFocusPrefab;
                if (isSetCurrentTile)
                    TilePalette.Instance.currentAnimationTileSource = this;
                break;
            case TileSourceType.OTHER:
                focusGO = TilePalette.Instance.otherSourceFocusPrefab;
                if (isSetCurrentTile)
                    TilePalette.Instance.currentOtherTileSource = this;
                break;
        }
        ObjectPool.Instance.DisableGameObjects(focusGO);
        if (gameObject.activeInHierarchy)
            StartCoroutine(Delay());
        else ObjectPool.Instance.GetGameObject(focusGO, this.transform, focusContainer);
        IEnumerator Delay()
        {
            yield return null;
            ObjectPool.Instance.GetGameObject(focusGO, this.transform, focusContainer);
        }
    }
}
[System.Serializable]
public class TileSourceData
{
    public string type, id;
    public static string GetOtherId()
    {
        int count = 0;
        DataController.Instance.gameData.tileSourceDataList.ForEach(tSDItem =>
        {
            if (tSDItem.type == TileSourceType.OTHER)
                count++;
        });
        return $"OtherData_{count.ToString()}";
    }
    public OtherData otherData;
}
[System.Serializable]
public class OtherData
{
    public float moveCost;
    public string type = "Normal";
    public bool canStand = true;
    public float height;
}

[System.Serializable]
public class TileSourceType
{
    public const string FLOOR = "Floor";
    public const string ANIMATION = "Animation";
    public const string DECORATION = "Decoration";
    public const string OTHER = "Other";
}