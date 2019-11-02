using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 一字ずつ出力されるようにするコード

public class PrologueText : MonoBehaviour {

    [SerializeField] private Text txt;
    static private string dialogText;
    [SerializeField] private GameObject Prologue;

    // 対話ウィンドウアニメーション
    private Animator anim;
    static int hashOpen = Animator.StringToHash("bIsOpen");

    WaitForSeconds waitSeconds = new WaitForSeconds(0.02f);
    WaitForSeconds NextDialogueWaitSeconds = new WaitForSeconds(1.0f);

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.enabled = false;
    }

    // CharacterPrologueScriptから対話を受ける
    public void GetText(string _prologue)
    {

        // アニメーターの実行、会話を保存
        anim.enabled = true;
        dialogText = _prologue;

        // チャットウィンドウがなければアニメーションでチャットウィンドウを開き、あったらすぐチャット開始
        if (!anim.GetBool(hashOpen))
            anim.SetBool(hashOpen, true);
        else
            ProloguePlay();
    }

    public void ProloguePlay()
    {
        StartCoroutine(NextPrologue());
    }

    // チャット開始
    IEnumerator NextPrologue()
    {
        // bIsCompleteDialogueはせりふがすべて完了していないという意味でfalseにする。
        UQGameManager.Instance.bIsCompleteDialogue = false;
        txt.text = "";          // 空白文字列から一字ずつ出力

        // 一字ずつ書き下す
        for (int i = 0; i < dialogText.Length; i++)
        {
            // 画面をタッチすると、片方の会話が進行中であれば、一文字ずつ書き下す効果を消す
            if (UQGameManager.Instance.bIsDialogueStop)
            {
                txt.text = dialogText;
                break;
            }
            else        // 特に入力がなければ、続けて一字ずつ出力
            {
                txt.text += dialogText[i];
                yield return waitSeconds;
            }
        }

        // せりふ終了
        UQGameManager.Instance.bIsCompleteDialogue = true;

        // せりふが終わった状態でタッチをすると次のせりふを開始し、Autoボタンを押すと自動的に次のせりふが進められる。
        do
        {
            if(UQGameManager.Instance.bIsAuto)
            {
                yield return NextDialogueWaitSeconds;
                Prologue.GetComponent<CharacterPrologueScript>().Bz();
                break;
            }
            if(UQGameManager.Instance.bIsNextDialogue)
            {
                Prologue.GetComponent<CharacterPrologueScript>().Bz();
                break;
            }
            yield return null;
        } while (true);

    }

    // チャットウィンドウを閉じる
    public void ClosePrologue()
    {
        anim.SetBool(hashOpen, false);
    }

    // チャットウィンドウの初期化
    public void ClearDialogue()
    {
        txt.text = "";
        anim.enabled = false;
    }
}
