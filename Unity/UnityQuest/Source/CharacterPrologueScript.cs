using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 特定地点およびステージ開始時、Az関数が呼び出され、インデックスに合う会話開始

public delegate void ProgressHandler();

[System.Serializable]
public class Dialogue
{
    [TextArea(3,10)]
    public string dialogue;

    public bool bIsUnity;
}

[System.Serializable]  //  MonoBehaviourではないクラスについてInspectorに示すこと。
public class StringItem
{
    // イメージおよび表わす会話をインスペクター上で入力
    public Sprite UnityImage;
    public Sprite PartnerImage;
    public string UnityName;
    public string PartnerName;
    public Dialogue[] variable;
}

public class CharacterPrologueScript : MonoBehaviour {

    public event ProgressHandler Handler;

    public delegate void Callback();
    private Callback callback = null;

    [SerializeField] private GameObject[] InactivationUI;     // 活性化/不活性化させるすべてのUI
    [SerializeField] private GameObject UnityProlog;         // 左側Window
    [SerializeField] private GameObject BossProlog;          // 右側Window

    [SerializeField] private StringItem[] dialogue;

    static int CurrentDialogueIdex = -1;
    static int CurrentDialogue = 0;

    Image Unity, Boss;
    Text UnityName, PartnerName;

    GameObject Button;          // Auto Button

    private void Awake()
    {
        Unity = UnityProlog.transform.GetChild(1).GetComponent<Image>();
        UnityName = UnityProlog.transform.GetChild(2).GetComponent<Text>();
        Boss = BossProlog.transform.GetChild(1).GetComponent<Image>();
        PartnerName = BossProlog.transform.GetChild(2).GetComponent<Text>();

        Button = transform.GetChild(2).gameObject;
        if (Button.activeSelf)
            Button.SetActive(false);
    }

    // 対話信号を受ける
    public void Az(int stringIndex)
    {
        // 信号が入るとbIsAutoをfalseで、会話が進んでいるということからbIsDialogueをtrueにする
        UQGameManager.Instance.bIsAuto = false;
        UQGameManager.Instance.bIsDialogue = true;

        // 対話インデックスを受けて、やり取りするインデックスを0に、初期化する
        CurrentDialogue = stringIndex;
        CurrentDialogueIdex = 0;

        // Auto ボタン活性化
        if (!Button.activeSelf)
            Button.SetActive(true);

        // やり取りする関数を実行し、チャットウィンドウを除いた残りのUIを不活性化する
        Bz();
        InactivationUI = GameObject.FindGameObjectsWithTag("PlayUI");
        SetInactivationUI(false);        // 特定のUIを隠す

        #region 対話のイメージを設定する。 イメージがなければないまま出る
        if (dialogue[CurrentDialogue].UnityImage)
        {
            Unity.enabled = true;
            Unity.sprite = dialogue[CurrentDialogue].UnityImage;
            UnityName.enabled = true;
            UnityName.text = dialogue[CurrentDialogue].UnityName;
        }
        if (dialogue[CurrentDialogue].PartnerImage)
        {
            Boss.enabled = true;
            Boss.sprite = dialogue[CurrentDialogue].PartnerImage;
            PartnerName.enabled = true;
            PartnerName.text = dialogue[CurrentDialogue].PartnerName;
        }
        #endregion
    }

    // プロローグを除く全てのUIを不活性化
    public void SetInactivationUI(bool _active)
    {
        for (int i = 0; i < InactivationUI.Length; i++)
        {
            for(int j=0;j < InactivationUI[i].transform.childCount;j++)
                InactivationUI[i].transform.GetChild(j).gameObject.SetActive(_active);
        }
    }

    // 会話のやりとり
    public void Bz()
    {
        // bIsNextDialogueはセリフですべての文字が出力されると待つが、この時、タッチをすれば次のセリフに行かせる役割をする。
        // bIsDialogueStopは、セリフが一字ずつ出力中にタッチをするとtrueに変わり、いっぺんにそのセリフを出力する役割をする。
        // 新しい一会話が始まるので、これらをfalseにする。
        UQGameManager.Instance.bIsNextDialogue = false;
        UQGameManager.Instance.bIsDialogueStop = false;

        #region 決まっている対話の長さだけ実行する。
        if (dialogue[CurrentDialogue].variable.Length > CurrentDialogueIdex)
        {
            #region 対話にユニティのせりふか相手のせりふかを示すbool値によってどちらに出力するかを決める。
            if (dialogue[CurrentDialogue].variable[CurrentDialogueIdex].bIsUnity)
            {
                UnityProlog.GetComponent<PrologueText>().GetText(dialogue[CurrentDialogue].variable[CurrentDialogueIdex].dialogue);
                
            }
            else
            {
                BossProlog.GetComponent<PrologueText>().GetText(dialogue[CurrentDialogue].variable[CurrentDialogueIdex].dialogue);
            }
            CurrentDialogueIdex++;
            #endregion
        }
        else        // 対話が終わると、双方のチャットウィンドウを閉じるアニメーションを実行する。
        {
            UnityProlog.GetComponent<PrologueText>().ClosePrologue();
            BossProlog.GetComponent<PrologueText>().ClosePrologue();
            StartCoroutine(WaitPrologue());         // 原状復旧
        }
        #endregion
    }

    // UIウィンドウが原状復旧され、動けるようにする。
    IEnumerator WaitPrologue()
    {
        // 一定時間後、UI復旧、ユニティと相手側のイメージの不活性化、チャットウィンドウを不活性化する。
        yield return new WaitForSeconds(0.8f);
        SetInactivationUI(true);
        Boss.enabled = false;
        Unity.enabled = false;
        UQGameManager.Instance.bIsDialogue = false;
        if (callback != null)
            callback();

        if (Button.activeSelf)
            Button.SetActive(false);
    }

    // Autoボタンを押すと自動で対話を実行
    public void PlayAuto()
    {
        UQGameManager.Instance.bIsAuto = true;
    }

    public void SetCallback(Callback cal)
    {
        callback = cal;
    }
}
