using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// このモンスターはネズミを捕獲するモンスターです。
// ネズミを掴むと,進行方向に飛んでいきます。
// ネズミがすべてなくなるとプレーヤーを攻撃して飛んでいきます。

public class Eagle : MonsterAI
{
    private GameObject HitTarget;           // 攻撃ターゲット
    private GameObject[] HuntTarget;        // ネズミ配列
    private GameObject CatchMouse;          // ターゲット ネズミ
    private GameObject Target;              // ターゲット

    public Transform CatchPos;              // ネズミをつける位置
    public float FlySpeed;

    private IDamage idamage;                // ダメージインターフェース

    private bool bIsRage;                   // プレーヤーについての復讐
    private bool bIsPeck;                   // プレーヤー攻撃
    private bool bIsCatch;                  // 捕獲成功

    WaitUntil wait;                         // 時間中止の時,待機

    private void Start()
    {
        Target = null;
        CatchMouse = null;
        wait = new WaitUntil(() => UQGameManager.Instance.bStop == false);
        HuntTarget = GameObject.FindGameObjectsWithTag("Mouse");
    }

    // 非活性化
    void InactivationMonster()
    {
        ObjectPool.Instance.PushToPool("Eagle", gameObject);
        StopCoroutine(Chase());
    }

    // 初期化
    protected override void OnEnable()
    {
        base.OnEnable();
        bIsCatch = false;
        state = MonsterState.Idle;
        StartCoroutine(FSMMain());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    // 鷲が獲物を探します。
    protected override IEnumerator Idle()
    {
        float delayTime = 2.0f;                         // 待機時間
        bool bPosition = false;                         // 位置
        float downValue = 0.005f;                       // 位置まで降りてくる速度
        float CurrentYPos = transform.position.y;
        yield return null;

        Target = FindTarget(HuntTarget);        // ターゲット

        do
        {
            // 指定位置まで降ります
            if (!bPosition)
            {
                do
                {
                    transform.position = new Vector3(transform.position.x, CurrentYPos, transform.position.z);
                    CurrentYPos -= downValue;
                    if (CurrentYPos < 0.9f)
                        bPosition = true;
                    yield return null;
                } while (!bPosition);
            }
            // 獲物を眺めて、なければプレーヤーを攻撃します。
            else
            {
                delayTime -= Time.deltaTime;

                if (!Target)
                {
                    delayTime = 2.0f;

                    // プレーヤーを攻撃します。
                    if (LengthArray(HuntTarget) == 0)
                        SetState(MonsterState.Chase);

                    Target = FindTarget(HuntTarget);
                }
                else
                {
                    if (transform.position.x > Target.transform.position.x)
                    {
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    if (delayTime <= 0.0f)
                    {
                        SetState(MonsterState.Hunt);
                    }
                }

            }
            yield return null;
        } while (!bIsNewState);
    }

    // 狩りします
    protected override IEnumerator Hunt()
    {
        float time = 2.0f;                          // 取って飛んで行くの時間
        Vector3 AngleVector = Vector3.zero;         // 飛んでいくのベクトル(方向)
        Vector3 subVector = Vector3.zero;
        yield return null;

        do
        {
            if (UQGameManager.Instance.bStop)
                yield return wait;

            // ネズミを向けて飛んで行きます。
            if (!bIsCatch)
            {
                if (AngleVector.sqrMagnitude == 0)
                {
                    subVector = Target.transform.position - transform.position;
                    AngleVector = new Vector3(subVector.x, -subVector.y, subVector.z);
                }

                transform.position = Vector2.MoveTowards(transform.position, Target.transform.position, subVector.normalized.sqrMagnitude * FlySpeed);

                // 再び獲物を探します。
                if (!Target.activeSelf)
                {
                    SetState(MonsterState.Idle);

                    if (!Target)
                        SetState(MonsterState.Chase);
                }

            }
            // ネズミを握って飛んで行きます。
            else
            {
                transform.position += (AngleVector.normalized * FlySpeed);
                time -= Time.deltaTime;
                if (time <= 0.0f)
                {
                    CatchMouse.transform.parent = null;
                    CatchMouse.gameObject.GetComponent<Mouse>().InactivationMonster();
                    UQGameManager.Instance.Monsters[enemyInfo.GetMonsterNumber()] = true;
                    ObjectPool.Instance.PushToPool("Eagle", gameObject);
                }
            }

            yield return null;
        } while (!bIsNewState);
    }

    // プレーヤーを攻撃します。
    protected override IEnumerator Chase()
    {
        Vector3 AngleVector = Vector3.zero;                         // 飛んでいくのベクトル(方向)
        Color32 rageColor = new Color32(255, 155, 155, 255);
        Vector3 subVector = Vector3.zero;
        float time = 2.0f;                                          // 攻撃後,飛んで行くの時間
        yield return null;

        bIsRage = true;
        HitTarget = GameObject.FindGameObjectWithTag("Player");
        gameObject.GetComponent<SpriteRenderer>().color = rageColor;

        do
        {
            if (UQGameManager.Instance.bStop)
                yield return wait;

            // プレーヤーを追いかけます。
            if (!bIsPeck)
            {
                // 方向
                if (transform.position.x > HitTarget.transform.position.x)
                    transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    transform.localScale = new Vector3(-1f, 1f, 1f);

                if (AngleVector.sqrMagnitude == 0)
                {
                    subVector = HitTarget.transform.position - transform.position;
                    AngleVector = new Vector3(subVector.x, -subVector.y, subVector.z);
                }

                transform.position = Vector2.MoveTowards(transform.position, HitTarget.transform.position, subVector.normalized.sqrMagnitude * FlySpeed);
            }
            // 攻撃後,飛んで行きます。
            else
            {
                transform.position += (AngleVector.normalized * FlySpeed);

                time -= Time.deltaTime;
                if (time <= 0.0f)
                {
                    UQGameManager.Instance.Monsters[enemyInfo.GetMonsterNumber()] = true;
                    InactivationMonster();
                }
            }
            yield return null;
        } while (!bIsNewState);
    }

    // ネズミ捕りとプレーヤー攻撃
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Mouse"))
        {
            CatchMouse = collision.gameObject;
            CatchMouse.GetComponent<BoxCollider2D>().enabled = false;
            CatchMouse.GetComponent<Rigidbody2D>().simulated = false;

            CatchMouse.GetComponent<Mouse>().SetState(MonsterState.Idle);
            CatchMouse.transform.parent = transform;
            CatchMouse.transform.position = CatchPos.position;

            box2d.enabled = false;

            bIsCatch = true;
        }
        else if (collision.CompareTag("Player"))
        {
            if (bIsRage)
            {
                bIsPeck = true;
                idamage = HitTarget.GetComponent<IDamage>();
                idamage.TakeDamage(enemyInfo.GetStr(), gameObject);
            }
        }

    }

    // 獲物を探します。
    GameObject FindTarget(GameObject[] Targets)
    {
        GameObject FindTarget = null;
        float distance = 0.0f;
        int startindex;

        if (Targets.Length < 1)
            return null;
        for (startindex = 0; startindex < Targets.Length; startindex++)
        {
            if (Targets[startindex].activeSelf)
            {
                FindTarget = Targets[startindex];
                distance = Vector2.Distance(transform.position, Targets[startindex].transform.position);
                break;
            }
        }

        // 最も近い獲物を探します。
        for (int i = startindex; i < Targets.Length; i++)
        {
            if (Targets[i].activeSelf)
            {
                if (distance > Vector2.Distance(transform.position, Targets[i].transform.position))
                {
                    distance = Vector2.Distance(transform.position, Targets[i].transform.position);
                    FindTarget = Targets[i];
                }
            }
        }
        return FindTarget;
    }

    // 獲物の数
    int LengthArray(GameObject[] Array)
    {
        int number = 0;

        for (int i = 0; i < Array.Length; i++)
        {
            if (Array[i].activeSelf) number++;
        }

        return number;
    }
}
