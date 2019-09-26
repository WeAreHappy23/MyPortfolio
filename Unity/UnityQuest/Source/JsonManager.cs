using UnityEngine;
using System.Collections;
using LitJson;
using System.IO;
using LitJson;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// ステージごとに持っている情報でモンスターを活性化、配置させます。

public delegate void NewSceneHandler();

// モンスター
public class CharactersInfo
{
    public int Stage;
    public string Name;
    public double HP;
    public double Str;
    public double X;
    public double Y;

    public CharactersInfo(int _stage, string _name, double _hp, double _str, double _x, double _y)
    {
        Stage = _stage;
        Name = _name;
        HP = _hp;
        Str = _str;
        X = _x;
        Y = _y;
    }
}

public class JsonManager : MonoBehaviour
{
    public event NewSceneHandler Handler;       // ステージ開始時,モンスターがすべて生成されたということを知らせてくれます。

    // 読み込みます。
    public void LoadBtn()
    {
        StartCoroutine(LoadInfoData());
    }

    // ファイルを探してロードします。
    IEnumerator LoadInfoData()
    {
        TextAsset tasset = Resources.Load("document") as TextAsset;
        JsonData itemData = JsonMapper.ToObject(tasset.ToString());
        GetItem(itemData);
        yield return null;
    }

    // 特定ステージにモンスターを設定、配置させます。
    private void GetItem(JsonData name)
    {
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

    // 特定のシーンには生成しません。
    private void OnLevelWasLoaded(int level)
    {
        if(UQGameManager.Instance.Stage != 0 && UQGameManager.Instance.Stage != -1)
            StartCoroutine(LoadInfoData());
    }

}


