using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 剣やスキルが持っています。'IDamage'を利用してダメージを与えます。

public class SkillDamage : MonoBehaviour
{
    public float Damage;              // ダメージ
    public GameObject Causer;         // スキルを使用したキャラクター
    private GameObject Effect;        // エフェクト
    IDamage damageInterface;

    private void Awake()
    {
        if (!Causer)
            Causer = GameObject.FindGameObjectWithTag("Player");
    }

    // インタフェースでダメージを与えます。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Mouse") || collision.CompareTag("Boss"))
        {
            // エフェクト
            Effect = ObjectPool.Instance.PopFromPool("HitEffect");
            Effect.transform.position = collision.gameObject.transform.position;

            // ダメージを与えます。
            damageInterface = collision.GetComponent<IDamage>();

            // キャスティングスキルの場合,完了すれば,基本より1.5倍のダメージを与えます。
            if (UQGameManager.Instance.bIsFullCastingSkill)
                damageInterface.TakeDamage(Damage * 1.5f + UQGameManager.Instance.SkillBuff, Causer);

            else
                damageInterface.TakeDamage(Damage + UQGameManager.Instance.SkillBuff, Causer);

        }
    }
}
