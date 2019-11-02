using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonsterAI
{
    [SerializeField] private string ObjectName;
    private GameObject HitTarget;
    private IDamage idamage;

    static int hashMove = Animator.StringToHash("speed");
    public Transform CollisionDetection;
    public Transform CollisionDetectionBack;
    [SerializeField] private LayerMask ColliderMask;
    [SerializeField] private LayerMask GroundMask;

    WaitForSeconds delay = new WaitForSeconds(0.5f);        // 死ぬ時の待ち時間
    Vector3 bossDirection = Vector3.zero;       // プレイヤーの位置を比較して左に行くか右に行くか決める。
    GameObject skillSeed;

    protected override void Awake()
    {
        base.Awake();
        enemyInfo.SetHP(enemyInfo.GetHP() + (enemyInfo.GetHP() * UQGameManager.Instance.ClearNum * 0.5f));

        anim.SetLayerWeight(1, 1.0f);
        anim.SetLayerWeight(2, 0.0f);

        enemyInfo.target = GameObject.FindGameObjectWithTag("Player");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        anim = GetComponent<Animator>();
        state = MonsterState.Chase;
        anim.SetLayerWeight(1, 1.0f);
        anim.SetLayerWeight(2, 0.0f);
        StartCoroutine(FSMMain());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    // モンスター無効化。再びオブジェクトプールに入れる。
    void InactivationMonster()
    {
        gameObject.SetActive(false);
        StopCoroutine(state.ToString());
    }

    protected override IEnumerator Idle()
    {
        yield return null ;
    }

    // プレイヤーと遠く離れていると一定確率でジャンプをしてしジャンプディレイもある。
    // プレイヤーと一定距離内にいると攻撃に変え(一般攻撃とスキル攻撃)、プレイヤーと距離が離れたら追跡状態で
    protected override IEnumerator Chase()
    {
        int jumpRandom = 2;							// ジャンプ確率
        float jumpDelay = 5.0f;
        float CurrentjumpDelay = 0.0f;
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

                // ターゲットとの位置(x)を比較して左追跡か右追跡か方向を決める。
                if (enemyInfo.target.transform.position.x > transform.position.x)
                {
                    bossDirection = Vector3.right;
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    bossDirection = Vector3.left;
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }

            if (bIsChasing)
            {
                // 一方向の追撃時間が終わるとまたランダム追撃時間をもらう。
                if (time <= 0.0f)
                    bIsChasing = false;
                else
                    time -= Time.deltaTime;

                if (CurrentjumpDelay > 0.0f)
                    CurrentjumpDelay -= Time.deltaTime;

                // 特定の距離にいると一定確率でジャンプでアクセス、特定の街の中にいると攻撃に転換
                RaycastHit2D groundInfoHorizontal = Physics2D.Raycast(CollisionDetection.position, bossDirection, 2.0f, ColliderMask);
                if (groundInfoHorizontal)
                {
                    if (groundInfoHorizontal.distance >= 1.0f && groundInfoHorizontal.distance <= 1.5f)
                    {
                        if (jumpRandom > Random.Range(1, 101))
                        {
                            if (CurrentjumpDelay <= 0.0f)
                            {
                                CurrentjumpDelay = jumpDelay;
                                anim.SetTrigger("bIsJump");
                                rig2d.velocity = new Vector2(bossDirection.x * 2.0f, 4.0f);
                            }
                        }
                    }
                    else if (groundInfoHorizontal.distance <= 0.1f)
                        SetState(MonsterState.Attack);
                }

                transform.Translate(new Vector3(bossDirection.x * Time.deltaTime * speed * 0.05f, 0, 0));
                anim.SetFloat(hashMove, transform.position.magnitude);

            }
            yield return null;
        } while (!bIsNewState);
    }

    // 一般攻撃、スキル攻撃
    protected IEnumerator Attack()
    {
        float skill_ = 20.0f;                         // スキルの確率
        float skillCoolTime = 5.0f;
        float skillTime = skillCoolTime;         // スキルプレイ時間プレー

        yield return null;

        do
        {
            RaycastHit2D HorizontalCheck = Physics2D.Raycast(CollisionDetection.position, bossDirection, 1.0f, ColliderMask);
            if (!HorizontalCheck)
            {
                if (skill_ > Random.Range(0, 101))       // ボスの後ろの方に移動するとき、ボスがスキルを使う確率
                {
                    // スキル実行
                    anim.SetBool("bIsCrouch", true);
                    do
                    {
                        if (skillTime < 0.0f)
                        {
                            anim.SetBool("bIsCrouch", false);
                            skillTime = skillCoolTime;
                            break;
                        }
                        else
                            skillTime -= Time.deltaTime;

                        yield return null;
                    } while (true);
                }
                else
                    SetState(MonsterState.Chase);
            }
            else
            {
                // 一般攻撃
                if (HorizontalCheck.distance < 0.1f)
                {
                    anim.SetTrigger("bIsAttack");
                    yield return delay;
                }
                else if (HorizontalCheck.distance > 0.1f)
                    SetState(MonsterState.Chase);
            }
            yield return null;

        } while (!bIsNewState);
    }

    // 土地からランダムな位置に物体が発射される
    public void BossSkill()
    {
        float forward, back;

        // 壁がある場合に備えてマップ内のみスキルが発生するようにする。
        RaycastHit2D SkillHorizontal = Physics2D.Raycast(CollisionDetection.position, bossDirection, 2.0f, GroundMask);
        RaycastHit2D SkillHorizontalBack = Physics2D.Raycast(CollisionDetectionBack.position, bossDirection * (-1), 2.0f, GroundMask);
        if (SkillHorizontal)
        {
            if (SkillHorizontal.distance > 0.3f)
                forward = SkillHorizontal.distance;
            else
                forward = 0.0f;
        }
        else
            forward = 2.0f;

        if (SkillHorizontalBack)
        {
            if (SkillHorizontalBack.distance > 0.3f)
                back = SkillHorizontalBack.distance;
            else
                back = 0.0f;
        }
        else
            back = 2.0f;

        // 5つの物体を発射させる
        float ran;
        for (int i = 0; i < 5; i++)
        {
            if (transform.localScale.x > 0)
                ran = Random.Range(-back, forward);
            else
                ran = Random.Range(-forward, back);

            // オブジェクトプールでランダムな位置に生成
            skillSeed = ObjectPool.Instance.PopFromPool("BossSeed");
            skillSeed.transform.position = new Vector3(transform.position.x + ran, -0.4f, 0.0f);
            skillSeed.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 150.0f);
        }
    }

    public void EndSkill()
    {
        anim.SetBool("bIsCrouch", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HitTarget = collision.gameObject;
            idamage = HitTarget.GetComponent<IDamage>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HitTarget = null;
            idamage = null;
        }
    }
}



