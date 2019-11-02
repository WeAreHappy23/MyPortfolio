using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSMでAI制作
// Coroutinで状態に応じた行動を見せることができる

public enum MonsterState { Idle, Move, Chase, Attack, Hunt }

public class MonsterAI : MonoBehaviour
{
    // 各モンスターが共通に必要な変数
    protected static int hashDeath = Animator.StringToHash("bIsDeath");
    protected WaitForSeconds delay = new WaitForSeconds(0.5f);
    public float speed;                  // 이동속도
    protected int direction;            // 방향

    // 状態
    protected MonsterState state;
    protected MonsterState InitialState;
    protected bool bIsNewState = false;

    // 参照コンポーネント及びスクリプト
    protected Rigidbody2D rig2d;
    public Animator anim;
    protected EnemyInfo enemyInfo;
    protected BoxCollider2D box2d;

    protected virtual void Awake()
    {
        // 初期化
        anim = GetComponent<Animator>();
        box2d = GetComponent<BoxCollider2D>();
        rig2d = GetComponent<Rigidbody2D>();
        enemyInfo = GetComponent<EnemyInfo>();

        InitialState = state;
    }

    protected virtual void OnDisable()
    {
        // モンスター除去
        // オブジェクトプールを使うので元の情報通りに戻るコード
        enemyInfo.SetHP(enemyInfo.GetInitialHP());
        enemyInfo.target = null;
        enemyInfo.DeathHandler -= SetDeathState;
        state = InitialState;
    }

    // デリゲートでイベントを適用
    protected virtual void OnEnable()
    {
        box2d.enabled = true;

        if (!enemyInfo.BIsBoss())
            enemyInfo.life = 1;
        else
            enemyInfo.life = 2;

        enemyInfo.DeathHandler += SetDeathState;
    }

    public void SetIdleCoroutine()
    {
        state = MonsterState.Idle;
        StartMyCoroutin();
    }

    public void StartMyCoroutin()
    {
        StartCoroutine(FSMMain());
    }

    public void SetDeathState()
    {
        box2d.enabled = false;
        anim.SetTrigger("bIsDeath");
        StopAllCoroutines();
    }

    protected IEnumerator FSMMain()
    {
        while (true)
        {
            bIsNewState = false;
            yield return StartCoroutine(state.ToString());
        }
    }

    public void SetState(MonsterState _state)
    {
        state = _state;
        bIsNewState = true;
    }

    // Idle
    protected virtual IEnumerator Idle()
    {
        yield return null;
    }

    // Move
    protected virtual IEnumerator Move()
    {
        yield return null;
    }

    // Chase
    protected virtual IEnumerator Chase()
    {
        yield return null;
    }

    // Attack
    protected virtual IEnumerator Attack()
    {
        yield return null;
    }

    // Hunt (Eagleの固有のAI)
    protected virtual IEnumerator Hunt()
    {
        yield return null;
    }
}
