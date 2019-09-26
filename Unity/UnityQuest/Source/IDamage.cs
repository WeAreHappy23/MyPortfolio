using UnityEngine;

// キャラクターとモンスターたちは'TakeDamage'という関数を持っています。
// 攻撃するオブジェクトがIDamageでダメージを与えれば、相手オブジェクトは自分が持っているTakeDamageでダメージを受けます。

public interface IDamage {

    // _damage - 攻撃の ダメージ / _cause - 攻撃するオブジェクト
    void TakeDamage(float _damage, GameObject _cause);
}
