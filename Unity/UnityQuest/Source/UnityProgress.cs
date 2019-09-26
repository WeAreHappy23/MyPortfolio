using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ステージが始まると,画面上段に現在位置が表示されます。

public class UnityProgress : MonoBehaviour {

    private Slider slider;

    // スタート位置,エンド位置,キャラクター
    private GameObject StartObject;
    private GameObject FinishObject;
    private GameObject Player;

    // 現在,全体の長さ
    private float Current;
    private float FullSize;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // プレーヤー,現在,エンドのオブジェクトを探します。
        Player = GameObject.FindGameObjectWithTag("Player");
        StartObject = GameObject.FindGameObjectWithTag("Respawn");
        FinishObject = GameObject.FindGameObjectWithTag("TeleportObject");
    }

    void Update()
    {
        // 現在の長さは(プレイヤーのX) - (スタート位置のX)です。その値をsliderに表示させます。
        if (Player && StartObject)
        {
            Current = Player.transform.position.x - StartObject.transform.position.x;

            slider.value = Current / FullSize;
        }
    }

    // シーンが開始されれば,特定ステージを除いて再びスタート位置,エンド位置,プレーヤーのオブジェクトを探します。
    private void OnLevelWasLoaded(int level)
    {
        // 特定ステージを除きます。
        if (UQGameManager.Instance.Stage == 7 || UQGameManager.Instance.Stage == 8 || UQGameManager.Instance.Stage == 9)
        {
            gameObject.SetActive(false);
            return;
        }

        // オブジェクトを探します。
        if (UQGameManager.Instance.Stage != 0 && UQGameManager.Instance.Stage != -1)
        {
            StartObject = GameObject.FindGameObjectWithTag("Respawn");
            FinishObject = GameObject.FindGameObjectWithTag("TeleportObject");
            Player = GameObject.FindGameObjectWithTag("Player");
            FullSize = FinishObject.transform.position.x - StartObject.transform.position.x;
            Current = 0.0f;
        }
    }
}
