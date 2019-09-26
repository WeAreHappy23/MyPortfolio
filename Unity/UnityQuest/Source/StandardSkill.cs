using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 長く押すと,強くなるスキルです。

public class StandardSkill : SkillButton, IPointerDownHandler, IPointerUpHandler
{
    public GameObject CastingEffect;        // フルキャストエフェクト
    public GameObject Skill1;               // スキルオブジェクト

    private int stack;                      // スタック

    public bool bIsPress;
    public bool bCompleteCasting;           // フルキャスト

    protected override void Awake()
    {
        base.Awake();

        bIsPress = false;
        stack = 0;
        bCompleteCasting = false;
    }

    void Update()
    {
        // 押したときに間,実行します。
        if (bIsPress)
        {
            stack++;
            if(stack >= 60)
            {
                UQGameManager.Instance.bIsFullCastingSkill = true;       // キャスティング完了  

                if (!bCompleteCasting && (CastingEffect = ObjectPool.Instance.PopFromPool("CastingEffect")))
                {
                    CastingEffect.transform.position = Player.transform.position;
                    bCompleteCasting = true;                // キャスティング完了
                }
            }
        }

        // スキルの実行後,スキルのオブジェクトを非活性化します。
        if (Skill1.activeSelf)
        {
            if (!UQGameManager.Instance.bIsUsingCastingSkill)
                Skill1.SetActive(false);
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
        if (UQGameManager.Instance.bIsPlayingSpeicalSkillAnimation || UQGameManager.Instance.bIsDraggingSkill || UQGameManager.Instance.bIsUsingTouchSkill || UQGameManager.Instance.bIsUsingDragSkill
            || UQGameManager.Instance.bIsJumping || UQGameManager.Instance.bPressingSpecialSkill || UQGameManager.Instance.bPressingSkill2)
        {
            bCancel = true;
            return;
        }

        // キャンセルがないとき実行します。
        if (!bCancel)
        {
            bIsPress = true;
            UQGameManager.Instance.bIsCastingSkill1 = true;
            skillFilter.fillAmount = 0;
        }
    }

    // 離すの時
    public void OnPointerUp(PointerEventData eventData)
    {
        UQGameManager.Instance.bIsCastingSkill1 = false;        // フルキャスト初期化

        if (bCancel) return; 

        bIsPress = false;

        // ジャンプがないとき,実行します。
        if (!UQGameManager.Instance.bIsJumping)
        {
            // スキル実行
            ButtonImage.raycastTarget = false;
            UseSkill();
            Skill1.SetActive(true);
            bCompleteCasting = false;                                   // キャスティング完了解除
            UQGameManager.Instance.bIsUsingCastingSkill = true;         // キャスティングスキル実行
            UQGameManager.Instance.bIsUseExeSkill = true;               // スキル実行
            UQGameManager.Instance.bIsFullCastingSkill = false;
            stack = 0;
        }
    }

    // OnEnable このオブジェクトが非活性化されれば,再び初期化します。
    protected override void OnEnable()
    {
        base.OnEnable();

        if (UQGameManager.Instance.bIsCastingSkill1)
        {
            UQGameManager.Instance.bIsCastingSkill1 = false;
            bIsPress = false;
            stack = 0;
            ButtonImage.raycastTarget = true;
            bCompleteCasting = false;
            UQGameManager.Instance.bIsUsingCastingSkill = false;
            UQGameManager.Instance.bIsUseExeSkill = false;
            UQGameManager.Instance.bIsFullCastingSkill = false;
        }
    }

   
}
