using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アイテムが回転しながらドロップされ、地面にいるときに空中に浮いているコード

public class UQItem : MonoBehaviour {

	RaycastHit2D ray2d;         // 地面との衝突感知
    Quaternion rot;              // 最初の角度保存
    [SerializeField] private LayerMask mask;        // 特定コライダー感知

    private Rigidbody2D rig2d;

    // 属性
    bool bIsFalling;
	bool bOnGround;
	float acc;
	float height;

	// Use this for initialization
	void Start () {
        rig2d = transform.parent.GetComponent<Rigidbody2D>();
        rot = transform.rotation;
		acc = 0.0f;
        height = transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {

        // アイテムドロップ時の地面に衝突する間、回転する
        if (!bOnGround && !UQGameManager.Instance.bStop) {
            
			curH = transform.parent.position.y;
            if (rig2d.velocity.y < 0 )	bIsFalling = true;
			
			ray2d = Physics2D.Raycast (transform.position, Vector2.down, 1.0f, mask);
			if (ray2d && ray2d.distance <= 0.3f) {
				if(bIsFalling)
				{
                    transform.rotation = rot;
					bOnGround = true;
				}
			}
            else
            {
				transform.Rotate (0.0f, 0.0f, 10.0f);
			}
		} 
		else {
            //地面が衝突すると浮いている状態になる
            //Sin関数は曲線を描き、-1~1状態が繰り返されるが、これを活用して浮揚状態適用
            acc += Time.deltaTime*2;
			transform.localPosition = new Vector3(transform.localPosition.x, height + (Mathf.Sin (acc)*0.2f), 0.0f);
		}
	}

    // アイテムができるようになったら上に力を入れて上がるようにする。
    private void OnEnable()
    {
        transform.parent.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 100.0f);
    }

    private void OnDisable()
    {
        bOnGround = false;
        bIsFalling = false;
    }
}
