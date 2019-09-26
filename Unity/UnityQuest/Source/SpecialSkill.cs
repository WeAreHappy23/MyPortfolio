using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// オーラとカットシーンを見せてくれます。

public class SpecialSkill : SkillButton, IPointerDownHandler, IPointerUpHandler
{
    public GameObject SpecialSkillObj;         // スキルのオブジェクト
    private GameObject Aura;                   // オーラ

    static int hashSpecialSkill = Animator.StringToHash("SpecialSkillAnimation2");          // アニメ

    // 実行時間
    public float ExeTime;
    private float timer;
   
    private Camera cam;

    protected override void Awake()
    {
        base.Awake();
        
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
    }

    void Update()
    {
        // 一定時間,実行します。
        if (UQGameManager.Instance.bUsingSpecialSkill)
        {
            timer += Time.deltaTime;

            if (timer >= ExeTime)
            {
                timer = 0.0f;
                SpecialSkillObj.SetActive(false);
                UQGameManager.Instance.bUsingSpecialSkill = false;
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
        if (UQGameManager.Instance.bIsDraggingSkill
            || UQGameManager.Instance.bIsCastingSkill1 || UQGameManager.Instance.bPressingSkill2)
        {
            bCancel = true;
            return;
        }

        UQGameManager.Instance.bPressingSpecialSkill = true;

        skillFilter.fillAmount = 0;
    }

    // 離すの時
    public void OnPointerUp(PointerEventData eventData)
    {
        UQGameManager.Instance.bPressingSpecialSkill = false; 

        if (bCancel) return;

        // スキル実行
        if (bCanUseSkill && eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            ButtonImage.raycastTarget = false;
            UseSkill();

            SpecialSkillObj.SetActive(true);

            // 選択したカットシーンを活性化します。
            if (!UQGameManager.Instance.Cutin2)
                cam.transform.GetChild(0).gameObject.SetActive(true);
            else
                cam.transform.GetChild(1).gameObject.SetActive(true);

            // オーラを活性化します。
            Aura = ObjectPool.Instance.PopFromPool("SpecialSkillAura");
            Aura.transform.position = Player.transform.position;

            Player.GetComponent<Animator>().SetTrigger(hashSpecialSkill);         // アニメ
            UQGameManager.Instance.bIsUseExeSkill = true;
            UQGameManager.Instance.bIsPlayingSpeicalSkillAnimation = true;
            UQGameManager.Instance.bUsingSpecialSkill = true;
        }
    }
}
