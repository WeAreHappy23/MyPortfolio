using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 空から剣が落ちるスキルです。
// 特定地点をドラッグするともっと多くの剣が落ちることができます。

public class DragSkill : SkillButton, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Camera cam;

    // スキルの範囲
    public GameObject Skill3RageParticle;          // 範囲のパーティクル
    public GameObject[] Range;
    public int RangeCount = 40;                    // 範囲のパーティクルの数
    public LayerMask groundMask;                   // パーティクルのレイヤー

    // モーションやスキル発生のオブジェクト
    public GameObject Dragging;
    public GameObject FallingSword;

    // 実行時間
    public float ExeTime;               
    private float timer;

    private Vector3 SkillStartPosition;     // スキルアイコンの初期位置

    public bool bIsPress;                  // スキル押し
    private bool bReturn;                  // ドラッグイメージの位置の初期化 -> チャットの時
    public bool bIsPlayingMotion;
    private bool bIsOnUI;

    protected override void Awake()
    {
        base.Awake();
        
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;

        bIsPress = false;
        bIsOnUI = true;

        if (Dragging)
        {
            Dragging.SetActive(false);
            bIsPlayingMotion = false;
        }

        // パーティクル配列生成
        Range = new GameObject[RangeCount];
        for (int i = 0; i < RangeCount; i++)
        {
            if (Skill3RageParticle)
            {
                Range[i] = Instantiate(Skill3RageParticle, Vector3.zero, Skill3RageParticle.transform.rotation) as GameObject;
                Range[i].transform.parent = transform;
                Range[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        // モーションの実行の時,スキル実行
        if (bIsPlayingMotion)
        {
            timer += Time.deltaTime;

            // 実行時間オーバー / ジャンプの時,スキル中止->全ての条件を初期化します。
            if (timer >= ExeTime || UQGameManager.Instance.bStopSkill)
            {
                timer = 0.0f;
                UQGameManager.Instance.bIsUseExeSkill = false;                      // スキル中止
                Dragging.SetActive(false);                                          // モーション中止
                bIsPlayingMotion = false;
                UQGameManager.Instance.DragStack = 0.0f;                            // スタック初期化
                UQGameManager.Instance.bIsUsingDragSkill = false;                   // ドラッグスキル中止
                UQGameManager.Instance.FinalDragPos = Vector2.zero;                 // スキルのイメージの位置の初期化
                Player.GetComponent<BattleSpriteAction>().bskill3Sound = false;     // スキルのサウンド初期化
                UQGameManager.Instance.bStopSkill = false;
                FallingSword.SetActive(false);                                      // スキルの実行オブジェクト初期化
            }
        }
    }

    // クルタイムを示すためのコルーチン
    IEnumerator Cooltime()
    {
        bCoroutine = true;
        while (skillFilter.fillAmount > 0)
        {
            skillFilter.fillAmount -= 1 * Time.smoothDeltaTime / coolTime;

            yield return null;
        }
        bCanUseSkill = true;
        ButtonImage.raycastTarget = true;
        bCoroutine = false;
        yield break;
    }

    // 押した時
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // キャンセル条件
        if (UQGameManager.Instance.bIsPlayingSpeicalSkillAnimation
            || UQGameManager.Instance.bIsCastingSkill1 || UQGameManager.Instance.bOnMenu
            || UQGameManager.Instance.bPressingSpecialSkill || UQGameManager.Instance.bPressingSkill2)
        {
            bCancel = true;         // キャンセル
            return;
        }

        bReturn = false;
        skillFilter.fillAmount = 0;
        SkillStartPosition = transform.position;
    }

    // ドラッグ中の時
    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグ中, 対話する時, キャンセル
        if (bCancel || bReturn) return;

        ButtonImage.raycastTarget = false;

        if (!bCanUseSkill)  return;

        // イメージドラッグの位置
        transform.position = eventData.position;
        UQGameManager.Instance.bIsDraggingSkill = true;

        // UIをポインティングしているか確認
        bIsOnUI = GetBoolTouchUI(eventData);

        #region 範囲パーティクル表示
        int filp = 1;
        int filpValue = 0;
        if (!bIsOnUI)
        {
            for (int i = 0; i < RangeCount; i++)
            {
                if (i >= (int)(RangeCount * 0.5f))
                {
                    filp = -1;
                    filpValue = (int)(RangeCount * 0.5f);
                }

                Vector3 SkillStandardPosition = new Vector3(transform.position.x + (i - filpValue) * 20.0f * filp, 750.0f, transform.position.z);
                RaycastHit2D distanceFromGround = Physics2D.Raycast(cam.ScreenToWorldPoint(SkillStandardPosition), Vector2.down, 10, groundMask);

                Range[i].SetActive(true);
                Vector3 CreatePos = distanceFromGround.point;
                CreatePos.z = 0.0f;
                Range[i].transform.position = CreatePos;
            }
        }
        else
        {
            for (int i = 0; i < RangeCount; i++)
            {
                Range[i].SetActive(false);
            }
        }
        #endregion
    }

    // 離すの時
    public void OnPointerUp(PointerEventData eventData)
    {
        // ドラッグ中止
        UQGameManager.Instance.bIsDraggingSkill = false;

        if (bCancel) return;

        // 他のスキル実行 or 対話中の時に,離すと実行されること防ぐ
        if (UQGameManager.Instance.bIsUseExeSkill || bReturn)
        {
            transform.position = SkillStartPosition;
            ButtonImage.raycastTarget = true;
            return;
        }

        UQGameManager.Instance.FinalDragPos = transform.position;
        transform.position = SkillStartPosition;

        // UIをポインティングしているか確認
        bIsOnUI = GetBoolTouchUI(eventData);

        if (!bIsOnUI && !UQGameManager.Instance.bIsJumping)
        {
            // パーティクル非活性化
            for (int i = 0; i < RangeCount; i++)
                Range[i].SetActive(false);

            // スキル実行
            if (Dragging)
            {
                Dragging.SetActive(true);
                bIsPlayingMotion = true;
            }
            ButtonImage.raycastTarget = false;
            UseSkill();
            FallingSword.SetActive(true);
            UQGameManager.Instance.bIsUseExeSkill = true;   // スキル実行中

        }
        else
            ButtonImage.raycastTarget = true;
    }

    bool GetBoolTouchUI(PointerEventData eventData)
    {
        // UIをポインティングすればスキルを実行できないようにします
        if (EventSystem.current.IsPointerOverGameObject(eventData.pointerId) == false)
            return false;
        else
            return true;
    }

    // OnEnable このオブジェクトが非活性化されれば,再び初期化
    protected override void OnEnable()
    {
        base.OnEnable();

        bReturn = true;
        UQGameManager.Instance.bIsDraggingSkill = false;
        transform.position = SkillStartPosition;                // 이미지를 원위치로 
    }

    // OnDisable このオブジェクトが非活性化されれば,再び初期化
    protected override void OnDisable()
    {
        base.OnDisable();

        timer = 0.0f;
        ButtonImage.raycastTarget = true;
        UQGameManager.Instance.bIsUseExeSkill = false;
        Dragging.SetActive(false);
        bIsPlayingMotion = false;
        UQGameManager.Instance.DragStack = 0.0f;
        UQGameManager.Instance.bIsUsingDragSkill = false;
        UQGameManager.Instance.FinalDragPos = Vector2.zero;
        UQGameManager.Instance.bStopSkill = false;
        FallingSword.SetActive(false);
    }
}
