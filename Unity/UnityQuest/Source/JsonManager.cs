using UnityEngine;
using System.Collections;
using LitJson;
using System.IO;
using LitJson;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public delegate void NewSceneHandler();

// リソースファイルからモンスター情報を呼び込んで配置

public class CharactersInfo
{
    public int ID;
    public string Name;
    public double HP;
    public double Str;
    public double X;
    public double Y;

    public CharactersInfo(int _id, string _name, double _hp, double _str, double _x, double _y)
    {
        ID = _id;
        Name = _name;
        HP = _hp;
        Str = _str;
        X = _x;
        Y = _y;
    }
}

public class JsonManager : MonoBehaviour
{
    public event NewSceneHandler Handler;       // 新しいステージが始まると、モンスター配置が完了した後、そのステージでモンスターの数、モンスターが倒される時のデリゲート適用などを担当。

    public List<CharactersInfo> itemList = new List<CharactersInfo>();
    public List<CharactersInfo> MyInventory = new List<CharactersInfo>();

    public void LoadBtn()
    {
        StartCoroutine(LoadInfoData());
    }

    // 完了するまで実行
    IEnumerator LoadInfoData()
    {
            TextAsset tasset = Resources.Load("document") as TextAsset;
            JsonData itemData = JsonMapper.ToObject(tasset.ToString());
            GetItem(itemData);
            yield return null;
    }

    private void GetItem(JsonData name)
    {
        // 各モンスターの情報の保存
        GameObject monster;
        for (int i = 0; i < name.Count; i++)
        {
            if (int.Parse(name[i]["Stage"].ToString()) == UQGameManager.Instance.Stage)
            {
                if (monster = ObjectPool.Instance.PopFromPool(name[i]["Name"].ToString()))
                {
                    monster.GetComponent<EnemyInfo>().SetInfo(float.Parse(name[i]["HP"].ToString()), float.Parse(name[i]["Str"].ToString()), float.Parse(name[i]["Speed"].ToString()));
                    monster.transform.position = new Vector3(float.Parse(name[i]["X"].ToString()), float.Parse(name[i]["Y"].ToString()), 0.0f);
                }
            }
        }

        Handler();
    }

    // 次のシーンに移動後実行
    private void OnLevelWasLoaded(int level)
    {
        if(UQGameManager.Instance.Stage != 0 && UQGameManager.Instance.Stage != -1)
            StartCoroutine(LoadInfoData());
    }

}


