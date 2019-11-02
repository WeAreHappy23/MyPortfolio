using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataSaveLoad : MonoBehaviour {

    // 保存するキャラクター情報
    [Serializable]
    public class UQData
    {
        public int clearNum;
        public float StrBuff;
        public float CriticalBuff;
        public float SkillBuff;
        public bool[] Monsters;
    }

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        LoadData();
    }

    // 保存するデータを特定の経路に保存する
    public void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Application.persistentDataPath + "/UQData.dat");

        // UQDataに保存するデータを入れて保存
        UQData data = new UQData();
        data.Monsters = new bool[UQGameManager.Instance.Monsters.Length];

        data.clearNum = UQGameManager.Instance.ClearNum;
        data.StrBuff = UQGameManager.Instance.STRBuff;
        data.CriticalBuff = UQGameManager.Instance.CriticalBuff;
        data.SkillBuff = UQGameManager.Instance.SkillBuff;
        
        for (int i = 0; i < UQGameManager.Instance.Monsters.Length; i++)
        {
            data.Monsters[i] = UQGameManager.Instance.Monsters[i];
        }
        bf.Serialize(fs, data);
        
        fs.Close();
    }

    // 保存されたデータの読み出し
    public void LoadData()
    {
        // 特定の経路のファイルを呼び出す。
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs;

        // ファイルが存在したら開き、なければ新たに生成する。
        if (File.Exists(Application.persistentDataPath + "/UQData.dat"))
        {
            fs =File.Open(Application.persistentDataPath + "/UQData.dat", FileMode.Open);
        }
        else
        {
            fs = File.Create(Application.persistentDataPath + "/UQData.dat");
        }

        if (fs != null && fs.Length > 0)
        {
            // 読み込んだデータで情報を保存する
            UQData data = (UQData)bf.Deserialize(fs);

            UQGameManager.Instance.ClearNum = data.clearNum;
            UQGameManager.Instance.STRBuff = data.StrBuff;
            UQGameManager.Instance.CriticalBuff = data.CriticalBuff;
            UQGameManager.Instance.SkillBuff = data.SkillBuff;
            UQGameManager.Instance.Monsters = data.Monsters;
            for (int i = 0; i < UQGameManager.Instance.Monsters.Length; i++)
            {
                UQGameManager.Instance.Monsters[i] = data.Monsters[i];
            }
        }
        fs.Close();
    }
}
