using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2度の命があり、攻撃、碑石スキル、転がるスキルがある

public class RealBoss : MonsterAI
{
    public delegate void Callback();
    private Callback callback = null;

    [SerializeField] private float Phase2HP;

    Vector3 bossDirection = Vector3.zero;       // プレイヤーの位置を比較して左に行くか右に行くか決める。
    [SerializeField] private Transform CollisionDetection;
    [SerializeField] private LayerMask ColliderMask;
    static int hashMove = Animator.StringToHash("speed");
    static int hashAttack = Animator.StringToHash("tAttack");
    static int hashSkill1 = Animator.StringToHash("bSkill1");
    static int hashSkill2 = Animator.StringToHash("tSkill2");
    static int hashStun = Animator.StringToHash("bStun");
    static int hashGetup = Animator.StringToHash("tGetup");

    private float MaxHP;
    private bool bSkill;                // 攻撃時にスキルか判別
    private bool bCollision;          // 転がり、壁に衝突すると、WaitUntilから次のフレームに移行させる。
    private bool bAttack;             // 攻撃アニメーションが終わるとfalseにするが、その時WaitUntilから次のフレームに移行させる。
    private bool bSkill2;              // スキル2アニメが終わったらfalseにするが、その時WaitUntilから次のフレームに移行させる。
    private bool bPase2;             // 倒れて2フェーズ開始初期
    private bool bIsPlayPase2;      // 2フェーズ開始

    WaitForSeconds retrunDelay = new WaitForSeconds(0.55f);
    WaitForSeconds deathDelay = new WaitForSeconds(1.2f);
    WaitForSeconds stunDelay = new WaitForSeconds(2.5f);
    WaitUntil Attackwait;
    WaitUntil Skill2wait;
    WaitUntil Skill1wait;
    WaitUntil Phase2Wait;
    WaitUntil Phase2Play;

    [SerializeField] private GameObject[] RollPos;
    private CharacterPrologueScript dialogue;
    protected override void Awake()
    {
        base.Awake();

        // 情報初期化
        enemyInfo.SetHP(enemyInfo.GetHP() + (enemyInfo.GetHP() * UQGameManager.Instance.ClearNum * 0.5f));

        // 特定の条件をすり抜けるための条件定義
        Attackwait = new WaitUntil(() => bAttack == false);
        Skill2wait = new WaitUntil(() => bSkill2 == false);
        Skill1wait = new WaitUntil(() => bCollision == true);
        Phase2Wait = new WaitUntil(() => bPase2 == true);
        Phase2Play = new WaitUntil(() => bIsPlayPase2 == true);

        MaxHP = enemyInfo.GetHP();
        enemyInfo.target = GameObject.FindGameObjectWithTag("Player");
        state = MonsterState.Chase;

        dialogue = GameObject.FindGameObjectWithTag("Dialogue").GetComponent<CharacterPrologueScript>();
        dialogue.SetCallback(StartMyCoroutin);          // チャットウィンドウが終わったら、callback関数を呼び出してStartMyCoroutin関数を実行する。
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        anim = GetComponent<Animator>();
        state = MonsterState.Chase;
        StartCoroutine(FSMMain());
    }

    // StartMyCoroutin関数が実行されると、このCoroutineが実行される。
    protected override IEnumerator Idle()
    {
        // 2フェーズ開始しなかった場合、2フェーズ開始
        if (!bPase2)
        {
            dialogue.SetCallback(PlayPhase2);           // 呼び出す関数を変更する。 チャットウィンドウが終わると実行する。
            bSkill2 = false;
            bAttack = false;
            bCollision = true;
            // 一定時間待機後立ち上がる
            yield return stunDelay;
            anim.enabled = true;
            anim.SetTrigger(hashGetup);
            if (!UQGameManager.Instance.Monsters[enemyInfo.GetMonsterNumber()])
            {
                // Phase2Waitは立ち上がる時まで待待つ。 しばらく止めて会話を始める
                yield return Phase2Wait;
                anim.enabled = false;
                dialogue.Az(4);
                // 会話が終わるまで待つ
                yield return Phase2Play;
            }
            else
            {
                PlayPhase2();
                bPase2 = true;
            }
            // この艦首は、ボスHPの体力を満たす効果を出す関数を呼び出す。
            if (callback != null)
                callback();

        }
        else        // 2フェーズ開始後、死ぬと無効化
        {
            // 倒れるアニメーションの後無効化させる
            anim.SetTrigger("bIsDeath");
            yield return deathDelay;
            gameObject.SetActive(false);
        }
    }

    protected override IEnumerator Chase()
    {
        float time = 0.0f;                             // 一方向に追う時間
        bool bIsChasing = false;                   // 追撃時間が完了して次の追撃をするか
        yield return null;

        do
        {
            // 一方向に追撃をする時間を設定
            if (!bIsChasing)
            {
                time = Random.Range(0.3f, 0.8f);
                bIsChasing = true;

                // プレイヤーの位置を比較して左に行くか右に行くか決める。
                if (enemyInfo.target.transform.position.x > transform.position.x)
                {
                    bossDirection = Vector3.right;
                    transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                }
                else
                {
                    bossDirection = Vector3.left;
                    transform.localScale = new Vector3(-0.005f, 0.005f, 0.005f);
                }
            }

            if (bIsChasing)
            {
                // 一方向の追撃時間が終わるとまたランダム追撃時間をもらう。
                if (time <= 0.0f)
                    bIsChasing = false;
                else
                    time -= Time.deltaTime;

                RaycastHit2D groundInfoHorizontal = Physics2D.Raycast(CollisionDetection.position, bossDirection, 1.0f, ColliderMask);
                if (groundInfoHorizontal)
                {
                    if (groundInfoHorizontal.distance <= 0.3f)
                        SetState(MonsterState.Attack);
                }

                anim.SetFloat(hashMove, transform.position.magnitude);
                transform.Translate(new Vector3(bossDirection.x * Time.deltaTime * speed * 0.05f, 0, 0));
            }
            yield return null;
        } while (!bIsNewState);
    }

    protected override IEnumerator Attack()
    {
        int random;
        int index;
        float LeftDistance;
        float RightDistance;

        GameObject Stone;

        yield return null;

        LeftDistance = Vector2.Distance(RollPos[0].transform.position, transform.position);
        RightDistance = Vector2.Distance(RollPos[1].transform.position, transform.position);

        do
        {
            // 確率によってスキル攻撃か一般攻撃か判別
            random = Random.Range(0, 10);
            if (random < 3)
                bSkill = true;
            else
                bSkill = false;

            if (bSkill)
            {
                random = Random.Range(0, 2);
                if (random == 0)
                {
                    if (LeftDistance < RightDistance)
                        index = 0;
                    else
                        index = 1;
                    // プレイヤーの位置を比較して左に行くか右に行くか決める。
                    SetDirection(RollPos[index].transform.position);
                    do
                    {
                        transform.Translate(new Vector3(bossDirection.x * Time.deltaTime * speed * 0.05f, 0, 0));
                        yield return null;
                    } while (Mathf.Abs(transform.position.x - RollPos[index].transform.position.x) > 0.2f);

                    bossDirection *= -1;
                    transform.localScale = new Vector3(transform.localScale.x * (-1), 0.005f, 0.005f);

                    anim.SetBool(hashSkill1, true);
                    GetComponent<Rigidbody2D>().AddForce(bossDirection * 1500.0f);
                    yield return Skill1wait;
                    anim.SetBool(hashStun, true);
                    yield return stunDelay;
                    anim.SetBool(hashStun, false);
                    yield return retrunDelay;
                }
                else
                {
                    // 碑石のスキル
                    anim.SetTrigger(hashSkill2);
                    bSkill2 = true;
                    Stone = ObjectPool.Instance.PopFromPool("RealBossStone");
                    Stone.transform.position = new Vector3(enemyInfo.target.transform.position.x, 2.0f, 0.0f);
                    yield return Skill2wait;
                }
            }
            else
            {
                // 一般攻撃
                anim.SetTrigger(hashAttack);
                bAttack = true;
                yield return Attackwait;
            }


            RaycastHit2D groundInfoHorizontal = Physics2D.Raycast(CollisionDetection.position, bossDirection, 1.0f, ColliderMask);
            if (!groundInfoHorizontal || groundInfoHorizontal.distance <= 0.2f)
            {
                SetState(MonsterState.Chase);
            }

            yield return null;
        } while (!bIsNewState);
    }

    // 転がるスキルで壁に衝突するとカメラに衝突効果を与える
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("LimitL") || collision.gameObject.CompareTag("LimitR"))
        {
            if (anim.GetBool(hashSkill1))
            {
                iTween.ShakePosition(GameObject.FindGameObjectWithTag("MainCamera"), iTween.Hash("x", 0.05, "y", 0.05, "time", 1));
                bCollision = true;
                anim.SetBool(hashSkill1, false);
            }
        }
    }

    public void SetNoneAttack()
    {
        bAttack = false;
    }

    public void SetNoneSkill2()
    {
        bSkill2 = false;
    }

    public void SetPhase2()
    {
        bPase2 = true;
    }

    public bool BIsPhase2()
    {
        return bPase2;
    }

    // 2フェーズ実行
    public void PlayPhase2()
    {
        anim.enabled = true;
        box2d.enabled = true;
        bIsPlayPase2 = true;
        enemyInfo.SetHP(Phase2HP + (UQGameManager.Instance.ClearNum*100));
        SetState(MonsterState.Chase);
    }

    public void FallDown()
    {
        anim.enabled = false;
    }

    public void SetCollback(Callback _call)
    {
        callback = _call;
    }

    // 方向設定
    void SetDirection(Vector3 compare)
    {
        if (compare.x > transform.position.x)
        {
            bossDirection = Vector3.right;
            transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        }
        else
        {
            bossDirection = Vector3.left;
            transform.localScale = new Vector3(-0.005f, 0.005f, 0.005f);
        }
    }
}
