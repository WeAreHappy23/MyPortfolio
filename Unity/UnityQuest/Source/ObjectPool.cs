using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 生成と破壊ではなく、あらかじめ生成し、無効化させ、必要な時に活性化させて再使用する。
// メモリーの負担を減らす

public class ObjectPool : Singleton<ObjectPool> {

    public List<PooledObject> objectPool = new List<PooledObject>();        // 必要な個数のオブジェクト

    private void Awake()
    {
        for (int i = 0; i < objectPool.Count; ++i)
            objectPool[i].Initialize(transform);
    }

    public bool PushToPool(string itemName, GameObject item, Transform parent = null)
    {
        // オブジェクトを探してこのスクリプトのオブジェクトの子として設定する
        PooledObject pool = GetPoolItem(itemName);
        if (pool == null)
            return false;
        pool.PushToPool(item, parent == null ? transform : parent);
        return true;
    }

    // 特定のオブジェクトを返還する
    public GameObject PopFromPool(string itemName, Transform parent = null)
    {
        PooledObject pool = GetPoolItem(itemName);
        if (pool == null)
            return null;

        return pool.PopFromPool(parent);
    }

    // オブジェクトを返還する
    PooledObject GetPoolItem(string itemName)
    {
        // 名前が同じオブジェクトを探して返還する。
        for (int i = 0; i<objectPool.Count; ++i)
        {
            if (objectPool[i].poolItemName.Equals(itemName))
            {
                return objectPool[i];
            }
        }
        return null;
    }
}
