using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PooledObject
{
    [SerializeField] bool bCreateIfEmpty;                   // 空いているときに新たに生成するか
    public string poolItemName = string.Empty;         // オブジェクトの名前
    public GameObject prefab = null;                      // オブジェクトのPrefab
    public int poolCount = 0;                                // 生成するオブジェクト本数

    [SerializeField]  private List<GameObject> poolList = new List<GameObject>();

    public void Initialize(Transform parent = null)
    {
        for (int i = 0; i < poolCount; ++i)
            poolList.Add(CreateItem(parent));
    }

    // 特定のオブジェクトを入れて無効化する
    public void PushToPool(GameObject item, Transform parent = null)
    {
        item.transform.SetParent(parent);
        item.SetActive(false);
        poolList.Add(item);
    }

    public int GetListCount()
    {
        return poolList.Count;
    }

    // 特定のオブジェクトを取り出して活性化させる
    public GameObject PopFromPool(Transform parent = null)
    {
        if (poolList.Count == 0)
        {
            if (bCreateIfEmpty)
                poolList.Add(CreateItem(parent));
            else
                return null;
        }
        
        GameObject item = poolList[0];
        item.SetActive(true);
        poolList.RemoveAt(0);

        return item;
    }

    // Instantiateで生成して無効化させる。
    private GameObject CreateItem(Transform parent = null)
    {
        GameObject item = Object.Instantiate(prefab) as GameObject;
        item.name = poolItemName;
        item.transform.SetParent(parent);
        item.SetActive(false);

        return item;
    }
}
