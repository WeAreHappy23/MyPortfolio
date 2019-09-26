using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragSkillStack : MonoBehaviour, IDragHandler
{
    public GameObject Motion;

    // ドラッグ位置
    public Transform Touch1;
    public Transform Touch2;
    public Transform MiddleTouch;

    public bool bIsTouch1;
    public bool bIsTouch2;

    public float dragStack;

    // Use this for initialization
    private void OnEnable()
    {
        dragStack = 0.006f;
        UQGameManager.Instance.DragStack = 0.0f;
    }

    // ドラッグでbIsTouch1,bIsTouch2がtrueになるとスタックが増加します。
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

    // スタック増加
    void Dragging()
    {
        if (bIsTouch1 && bIsTouch2)
        {
            bIsTouch1 = false;
            bIsTouch2 = false;
            UQGameManager.Instance.DragStack += dragStack;
        }
    }
}
