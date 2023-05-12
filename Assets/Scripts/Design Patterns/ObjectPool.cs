using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Pixelplacement;
using Sirenix.OdinInspector;

/// <summary>
/// Found free GameObject in object pool.
/// </summary>
public class ObjectPool : Singleton<ObjectPool>
{
    [DictionaryDrawerSettings]
    [ShowInInspector]
    Dictionary<int, List<GameObject>> gODic = new Dictionary<int, List<GameObject>>();
    [Button]
    public GameObject GetGameObject(GameObject prefab, Transform newTransform = null, Transform parent = null)
    {
        newTransform = newTransform != null ? newTransform : prefab.transform;
        int key = prefab.GetInstanceID();
        GameObject result = null;

        if (!gODic.ContainsKey(key))
        {
            SpawnGO();
        }
        else
        {
            var value = new List<GameObject>();
            gODic.TryGetValue(key, out value);
            bool hasGetGO = false;
            for (int index = value.Count - 1; index >= 0; index--)
            {
                result = value[index];
                if (result != null)
                {
                    if (!result.activeSelf)
                    {
                        // Found free GameObject in object pool.
                        result.SetActive(true);
                        result.transform.position = newTransform.position;
                        result.transform.rotation = newTransform.rotation;
                        result.transform.localScale = newTransform.localScale;
                        SetParent();
                        hasGetGO = true;
                        break;
                    }
                }
                else Destroy(value[index]);
            }
            if (!hasGetGO)
                SpawnGO();
        }
        return result;

        void SetParent()
        {
            result.transform.parent = parent == null ? this.transform : parent;
        }
        void SpawnGO()
        {
            // Instantiate because there is no free GameObject in object pool.
            result = Instantiate(prefab, newTransform.position, newTransform.rotation);
            SetParent();
            result.transform.localScale = newTransform.localScale;
            List<GameObject> value = new List<GameObject>();
            if (gODic.ContainsKey(key))
            {
                gODic.TryGetValue(key, out value);
            }
            value.Add(result);
            gODic[key] = value;
        }
    }
    [Button]
    public void DisableGameObjects(GameObject prefab)
    {
        var key = prefab.GetInstanceID();
        var value = new List<GameObject>();
        gODic.TryGetValue(key, out value);
        if (value != null)
        {
            for (int index = value.Count - 1; index >= 0; index--)
            {
                if (value[index] != null)
                    value[index].SetActive(false);
                else value.Remove(value[index]);
            }
            gODic[key] = value;
        }
        else Debug.LogError("prefab is null");
    }
    public void DestroyAll()
    {
        foreach (var item in gODic)
        {
            item.Value.ForEach(gOItem => Destroy(gOItem));
        }
        gODic.Clear();
    }
}