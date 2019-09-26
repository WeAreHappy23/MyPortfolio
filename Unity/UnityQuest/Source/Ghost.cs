using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// このモンスターはモンスターを召喚します。

public class Ghost : MonsterAI
{
    private GameObject HitTarget;              // 攻撃ターゲット
    private IDamage idamage;                   // ダメージインターフェース
    private bool bIsHit;                       // このモンスターのコライダーの中にキャラクターがいる時

    static int hashHalo = Animator.StringToHash("bIsHalo");         // アニメ

    public GameObject[] SummonsMonster;                 // 召喚モンスター
    public Transform CollisionDetection;                // レイキャストの位置
    public Transform CollisionDetectionBack;            // レイキャストの位置(うしろ)
    public LayerMask ColliderMask;                      // コライダーのマスク
    public LayerMask ColliderMaskBack;                  // コライダーのマスク(うしろ)
    public float skillCoolTime;                         // 召喚スキルのクルタイム

    // 持続的にダメージを与えます。
    void Update()
    {
        if (bIsHit)
            idamage.TakeDamage(enemyInfo.GetStr(), gameObject);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        state = MonsterState.Move;
        StartCoroutine(FSMMain());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    // 非活性化
    void InactivationMonster()
    {
        bIsHit = false;
        ObjectPool.Instance.PushToPool("Ghost", gameObject);
        StopCoroutine(Chase());
    }

    // ランダム方向に移動します。
    protected override IEnumerator Move()
    {
        int Randomdirection = 0;                // ランダム方向
        float time = 0.0f;                      // 実行時間
        bool bIsResult = false;
        Vector3 direction = Vector3.zero;
        yield return null;

        do
        {
            // 進行中でないとき,方向を定めます。
            if (!bIsResult)
            {
                Randomdirection = Random.Range(-1, 2);
                time = Random.Range(1.0f, 3.0f);

                // ランダム方向に方向を変えます。
                if (Randomdirection == -1)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    direction = Vector3.left;
                }
                else if (Randomdirection == 1)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    direction = Vector3.right;
                }
                else
                    direction = Vector3.zero;

                bIsResult = true;       // 移動が行われるようにします。
            }

            // 移動します。
            if (bIsResult)
            {
                if (time <= 0.0f)
                    bIsResult = false;
                else
                    time -= Time.deltaTime;

                transform.Translate(direction * speed * Time.deltaTime * 0.05f);
            }

            // ターゲットが存在すれば,追跡状態に変えます。
            if (enemyInfo.target)
                SetState(MonsterState.Chase);

            yield return null;
        } while (!bIsNewState);
    }

    // 追跡状態です。
    protected override IEnumerator Chase()
    {
        float time = 0.0f;                               // 実行時間
        bool bIsChasing = false;                         // 追撃時間が完了し,次の追撃をするかどうか
        Vector3 direction = Vector3.zero;
        float currentSkillCoolTime = skillCoolTime;
        yield return null;

        do
        {
            // 一方向に追撃をする時間を設定します。
            if (!bIsChasing)
            {
                time = Random.Range(2.5f, 3.5f);
                bIsChasing = true;

                // 方向
                if (enemyInfo.target.transform.position.x > transform.position.x)
                {
                    direction = Vector3.right;
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    direction = Vector3.left;
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }

            if (bIsChasing)
            {
                // 決まった時間が経てばまた時間を設定します。
                if (time <= 0.0f)
                    bIsChasing = false;
                else
                    time -= Time.deltaTime;

                // 進行方向に障害物があれば,逆方向に移動します。
                RaycastHit2D groundInfoHorizontal = Physics2D.Raycast(CollisionDetection.position, direction, 1.0f, ColliderMask);
                if (groundInfoHorizontal)
                {
                    if (groundInfoHorizontal.distance < 0.01f)
                    {
                        direction *= -1;
                        time *= 0.5f;
                        float localScaleX = -transform.localScale.x;
                        transform.localScale = new Vector3(localScaleX, 1f, 1f);
                    }
                }

                transform.Translate(direction * speed * Time.deltaTime * 0.05f);

                // 特定体力以下になると,モンスターを召喚します。
                if (enemyInfo.GetHP() <= 200.0f)
                {
                    if (currentSkillCoolTime <= 0.0f)
                    {
                        SetState(MonsterState.Hunt);
                        currentSkillCoolTime = skillCoolTime;
                    }
                    else
                        currentSkillCoolTime -= Time.deltaTime;
                }

                // 非活性化
                if (enemyInfo.GetHP() <= 0.0f)
                {
                    anim.SetTrigger(hashDeath);
                    yield return delay;
                }
            }
            yield return null;
        } while (!bIsNewState);

        yield return null;
    }

    protected override IEnumerator Hunt()
    {
        float back, forward;                    // 前後の距離
        bool returnChase = true;                // 召喚モンスターがなければ,再び追跡状態
        Vector3 direction = Vector3.zero;
        yield return null;

        box2d.enabled = false;                  // 無敵状態
        anim.SetBool("bIsHalo", true);

        // 前,後ろの壁との距離を考慮してスケルトンを召喚します。
        direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left; 
        RaycastHit2D groundInfoHorizontal = Physics2D.Raycast(CollisionDetection.position, direction, 1.0f, ColliderMask);
        RaycastHit2D groundInfoHorizontalBack = Physics2D.Raycast(CollisionDetectionBack.position, direction * (-1), 1.0f, ColliderMaskBack);
        if (groundInfoHorizontal)
        {
            if (groundInfoHorizontal.distance > 0.3f)
                forward = groundInfoHorizontal.distance;
            else
                forward = 0.0f;
        }
        else
            forward = 1.0f;

        if (groundInfoHorizontalBack)
        {
            if (groundInfoHorizontalBack.distance > 0.3f)
                back = groundInfoHorizontalBack.distance;
            else
                back = 0.0f;
        }
        else
            back = 1.0f;

        float ran;

        // ランダム地点にスケルトンを召喚します。
        for (int i = 0; i < SummonsMonster.Length; i++)
        {
            if(transform.localScale.x >0)
                ran = Random.Range(-back, forward);
            else
                ran = Random.Range(-forward, back);
            SummonsMonster[i] = ObjectPool.Instance.PopFromPool("SummonSkeleton");
            SummonsMonster[i].transform.position = new Vector3(transform.position.x + ran , -0.09f, 0.0f);
        }

        // スケルトンがなくなれば追跡状態に...
        do
        {
            returnChase = true;

            // 召喚モンスターがあるかをチェックします。
            for (int i = 0; i< SummonsMonster.Length; i++)
            {
                if (SummonsMonster[i].activeSelf)
                {
                    returnChase = false;
                    break;
                }
            }

            // 追跡条件に,初期化します。
            if (returnChase)
            {
                for (int i = 0; i < SummonsMonster.Length; i++)
                {
                    SummonsMonster[i] = null;
                }
                anim.SetBool("bIsHalo", false);
                box2d.enabled = true;
                SetState(MonsterState.Chase);
            }
            yield return null;
        } while (!bIsNewState);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bIsHit = true;
            HitTarget = collision.gameObject;
            idamage = HitTarget.GetComponent<IDamage>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bIsHit = false;
            HitTarget = null;
            idamage = null;
        }
    }
}
