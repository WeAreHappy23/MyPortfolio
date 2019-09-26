using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// このスキルはスキルの実行後,タッチすればするほど,多くの剣が発射されるスキルです。

public class TouchSkill : SkillButton, IPointerDownHandler, IPointerUpHandler
{
    public GameObject Touching;           // モーション
    public GameObject Skill2;             // スキルオブジェクト

    // 実行時間
    public float ExeTime;
    float timer;

    public bool bIsPlayingMotion;

    protected override void Awake()
    {
        base.Awake();

        if (Touching)
        {
            Touching.SetActive(false);
            bIsPlayingMotion = false;
        }
    }

    void Update()
    {
        // モーション実行時,スキルを実行します。
        if (bIsPlayingMotion)
        {
            timer += Time.deltaTime;

            // 実行時間オーバー / ジャンプの時,スキル中止->全ての条件を初期化します。
            if (timer >= ExeTime || UQGameManager.Instance.bStopSkill)
            {
                Skill2.SetActive(false);
                timer = 0.0f;
                UQGameManager.Instance.bIsUseExeSkill = false; 
                Touching.SetActive(false);
                bIsPlayingMotion = false;
                UQGameManager.Instance.TouchStack = 0.0f;
                UQGameManager.Instance.bIsUsingTouchSkill = false;

                // スキル中止
                if (UQGameManager.Instance.bStopSkill)
                {
                    // スキルのオーラを非活性化します。
                    if (Player.transform.GetChild(2).gameObject.activeSelf)
                        Player.transform.GetChild(2).GetComponent<Skill2Aura>().SetInActivation();
                    UQGameManager.Instance.bStopSkill = false;
                }
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
        if (!bCanUseSkill || UQGameManager.Instance.bIsPlayingSpeicalSkillAnimation || UQGameManager.Instance.bIsUseExeSkill || UQGameManager.Instance.bOnMenu
            || UQGameManager.Instance.bIsCastingSkill1 || UQGameManager.Instance.bIsDraggingSkill || UQGameManager.Instance.bPressingSpecialSkill)
        {
            bCancel = true;
            return;
        }

        UQGameManager.Instance.bPressingSkill2 = true;
        skillFilter.fillAmount = 0;
    }

    // 離すの時
    public void OnPointerUp(PointerEventData eventData)
    {
        UQGameManager.Instance.bPressingSkill2 = false;

        if (bCancel) return;

        if (bCanUseSkill && eventData.pointerCurrentRaycast.gameObject == gameObject && !UQGameManager.Instance.bIsJumping) 
        {
            // スキル実行
            ButtonImage.raycastTarget = false;
            UseSkill();
            Skill2.SetActive(true);
            UQGameManager.Instance.bIsUseExeSkill = true;

            if (Touching)
            {
                Touching.SetActive(true);
                bIsPlayingMotion = true;
            }
        }

    }

    // このオブジェクトが非活性化されれば,再び初期化します。
    protected override void OnDisable()
    {
        base.OnDisable();

        Skill2.SetActive(false);
        timer = 0.0f;
        UQGameManager.Instance.bIsUseExeSkill = false;
        Touching.SetActive(false);
        bIsPlayingMotion = false;
        UQGameManager.Instance.TouchStack = 0.0f;
        UQGameManager.Instance.bIsUsingTouchSkill = false;

        // スキルのオーラを非活性化します。
        if (Player.transform.GetChild(2).gameObject.activeSelf)
            Player.transform.GetChild(2).GetComponent<Skill2Aura>().SetInActivation();
        UQGameManager.Instance.bStopSkill = false;
    }
}
