using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ゲームの土地の付近に3つのUIブロックを置いて、そこのブロックを全部過ぎてからドラッグすれば、多くの剣が落ちる

public class DragSkillStack : MonoBehaviour, IDragHandler
{
    public GameObject Motion;
    public Transform Touch1;
    public Transform Touch2;
    public Transform MiddleTouch;

    public bool bIsTouch1;
    public bool bIsTouch2;

    private Camera cam;

    public float dragStack;

    DragSkill dragSkill;
    // Use this for initialization
    private void OnEnable()
    {
        dragStack = 0.008f;
        UQGameManager.Instance.DragStack = 0.0f;
    }

    // 3つのブロックを全部過ぎるとスタックが溜まってこれを繰り返して多くのスタックを得ることができる
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.position.x < Touch1.position.x)
        {
            bIsTouch1 = true;
        }
        else if (eventData.position.x > Touch2.position.x)
        {
            bIsTouch2 = true;
        }
        else if(Vector2.Distance(eventData.position, MiddleTouch.position) < 100.0f )
        {
            Dragging();
        }
    }

    // 再びドラッグできるようにしてスタックが溜まる。
    void Dragging()
    {
        if (bIsTouch1 && bIsTouch2)
        {
            bIsTouch1 = false;
            bIsTouch2 = false;
            if (UQGameManager.Instance.DragStack < 0.05)
                UQGameManager.Instance.DragStack += dragStack;
        }
    }
}
