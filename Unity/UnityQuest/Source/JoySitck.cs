using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoySitck : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Transform PlayerCharacter;       // 플레이어캐릭터
    public Image Stick;     // 움직이는 조이스틱
    public Image BackGround;

    private Vector3 inputVector;
   
    // Use this for initialization
    void Start()
    {
        BackGround = GetComponent<Image>();
        Stick = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle
            (BackGround.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            Stick.rectTransform.anchoredPosition = new Vector3(inputVector.x * ((BackGround.rectTransform.sizeDelta.x - 120) / 2), 0.0f, 0.0f);

            pos.x = (pos.x / (BackGround.rectTransform.sizeDelta.x * 0.25f));
            pos.y = 0.0f;

            inputVector = new Vector3(pos.x , 0.0f, 0.0f);
            inputVector = (inputVector.magnitude > 1.0f ? inputVector.normalized : inputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector3.zero;
        Stick.rectTransform.anchoredPosition = Vector3.zero;

        if(UQGameManager.Instance.bIsMoving)             
            UQGameManager.Instance.bIsMoving = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UQGameManager.Instance.bIsMoving = true;         
        OnDrag(eventData);
    }

    public float Horizontal()
    {
        // 특정 스킬이 실행 중일 때, 각성 애니메이션이 실행 중일 때, 메뉴창이 있을 때 움직임 불가능
        if (UQGameManager.Instance.bIsUseExeSkill || UQGameManager.Instance.bIsPlayingSpeicalSkillAnimation || UQGameManager.Instance.bOnMenu || UQGameManager.Instance.bIsDialogue) return 0.0f;
        if (inputVector.x != 0)
            return inputVector.x;
        else
            return Input.GetAxis("Horizontal");
    }

    public float Vertical()
    {
        if (inputVector.z != 0)
            return inputVector.z;
        else
            return Input.GetAxis("Vertical");
    }

    // 대화가 끝나면 활성화 됨 -> 이동 중 대화를 시작하고 끝내면 조이스틱버튼이 고정되는 문제
    private void OnEnable()
    {
        UQGameManager.Instance.bIsMoving = false;
        inputVector = Vector3.zero;
        Stick.rectTransform.anchoredPosition = Vector3.zero;
    }
}
