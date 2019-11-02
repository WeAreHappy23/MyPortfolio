using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2枚の背景を使用して、引き続き繰り返される背景

public class BackGroundRepeat : MonoBehaviour {

    private GameObject Player;
    [SerializeField] private GameObject BackGround2;

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // プレイヤーが2枚のイメージの中間の間にいると無視
        if ((Player.transform.position.x < transform.position.x && Player.transform.position.x > BackGround2.transform.position.x) ||
            (Player.transform.position.x > transform.position.x && Player.transform.position.x < BackGround2.transform.position.x))
            return; 
        else
        {
            // イメージの中間を越える時、背景を再配置
            if (Player.transform.position.x > transform.position.x)
            {
                if (transform.position.x > BackGround2.transform.position.x)
                    Repeat();
            }
            else
            {
                if (transform.position.x < BackGround2.transform.position.x)
                    BackRepeat();
            }
        }
    }

    // 反復(前)
    void Repeat()
    {
        BackGround2.transform.position = new Vector2(BackGround2.transform.position.x + 9.3f , BackGround2.transform.position.y);
    }

    // 反復(後)
    void BackRepeat()
    {
        BackGround2.transform.position = new Vector2(BackGround2.transform.position.x - 9.3f, BackGround2.transform.position.y);
    }
}
