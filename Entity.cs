using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project.Lib {
	/// <summary>
	/// 実在クラス
	/// </summary>
    public abstract class Entity {
        protected GameObject gameObject_;
		public GameObject gameObject{ get { return gameObject_; } }
		protected Transform cacheTrans_;
		public Transform CacheTrans{ get { return cacheTrans_; } }

		public string ResName{ get; set; }

		/// <summary>
		/// インスタンスIdを識別子として使う
		/// </summary>
		public int GetId(){
			return gameObject_.GetInstanceID ();
		}

		public string Name { get { return gameObject_.name; } }

		/// <summary>
		/// インスタンス生成時処理
		/// </summary>
		public abstract Task<GameObject> CreateAsync(string s);

#if UNITY_EDITOR
		/// <summary>
		/// インスタンス生成時処理
		/// </summary>
		public virtual GameObject Create(GameObject resObject) {
			return resObject;
		}
#endif

		/// <summary>
		/// インスタンス破棄
		/// </summary>
		public abstract void Destroy();

		/// <summary>
		/// 初期化
		/// </summary>
		public virtual void Setup() {
		}
		/// <summary>
		/// 初期化
		/// </summary>
		public virtual void Cleanup() {
		}

		/// <summary>
		/// インスタンス破棄
		/// </summary>
		public void Release() {
			GameObject.Destroy(gameObject_);
			gameObject_ = null;
		}

		/// <summary>
		/// 自分のオブジェクト設定
		/// </summary>
		public void SetOwn(GameObject obj) {
            gameObject_ = obj;
            cacheTrans_ = obj.transform;
        }
		/// <summary>
		/// Active設定
		/// </summary>
		public void SetActive(bool isActive){
			gameObject_.SetActive (isActive);
		}
		/// <summary>
		/// gameObjectのActive判定
		/// </summary>
		public bool IsActive() {
            if(gameObject_ == null) {
                return false;
            }
			return gameObject_.activeSelf;
		}

		/// <summary>
		/// Entityの生存判定
		/// </summary>
		public virtual bool IsExist() {
			//基本的にgameobjectがfalseになったらインスタンス破棄する
			return IsActive();
		}

		/// <summary>
		/// 親になるTransform設定
		/// </summary>
		public void SetParent(Transform parent, bool worldPositionStay=true){
			cacheTrans_.SetParent(parent, worldPositionStay);
		}

		/// <summary>
		/// 子になるTransform設定
		/// </summary>
		public void SetChild(Transform child){
			child.SetParent(cacheTrans_);
		}

		/// <summary>
		/// レイヤーを設定
		/// </summary>
		public void SetLayer(int layer)
		{
			gameObject_.layer = layer;
		}
		/// <summary>
		/// レイヤーを設定
		/// </summary>
		/// <remarks>
		/// 子階層を含めてすべて設定
		/// </remarks>
		public void SetLayerInChildren(int layer) {
			Transform[] child = gameObject_.GetComponentsInChildren<Transform>();

			for (int i = 0, max = child.Length; i < max; i++){
				child[i].gameObject.layer = layer;
			}
		}

		/// <summary>
		/// 座標設定
		/// </summary>
		public void SetPosition(Vector3 pos) {
			cacheTrans_.localPosition = pos;
		}

		/// <summary>
		/// 座標取得
		/// </summary>
		public Vector3 GetPosition() {
			return cacheTrans_.localPosition;
		}

		/// <summary>
		/// 回転設定
		/// </summary>
		public void SetRotation(Quaternion rot) {
			cacheTrans_.localRotation = rot;
		}
		/// <summary>
		/// 回転取得
		/// </summary>
		public Quaternion GetRotation() {
			return cacheTrans_.localRotation;
		}
		/// <summary>
		/// スケール設定
		/// </summary>
		public void SetScale(Vector3 scl) {
			cacheTrans_.localScale = scl;
		}
		/// <summary>
		/// スケール取得
		/// </summary>
		public Vector3 GetScale() {
			return cacheTrans_.localScale;
		}

		/// <summary>
		/// 実行処理
		/// </summary>
		public virtual void Execute(){
		}

		/// <summary>
		/// Playable更新
		/// </summary>
		public virtual void Evaluate() {
		}

		/// <summary>
		/// 実行後処理
		/// </summary>
		public virtual void LateExecute(){
		}
        /// <summary>
        /// 物理計算実行処理
        /// </summary>
        public virtual void FixedExecute() {
        }
	}
}
