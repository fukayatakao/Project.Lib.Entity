using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Project.Lib {
	/// <summary>
	/// Entity管理(リソースなし)
	/// </summary>
	public abstract class EntityAssembly<T, U> : IEntityAssembly
            where T : Entity, new() where U : class, IEntityAssembly, new()
		{
        protected Transform cacheTrans_;
        // インスタンス保持用
        protected EntityStorage<T> storage_;

        protected virtual int DefaultInstanceMax { get { return 128; } }
		//インスタンスを破棄するときにストックに回して次のインスタンス作成時に使いまわすか
		public bool IsStock = true;
		/// <summary>
		/// アクティブなインスタンス
		/// </summary>
		public T[] Current { get { return storage_.Current; } }
		/// <summary>
		/// アクティブなインスタンスの数
		/// </summary>
		public int Count { get { return storage_.TailIndex; } }

		//有効フラグ
		private bool enable_ = true;

		/// <summary>
		/// 有効にする
		/// </summary>
		public void Enable() {
			enable_ = true;
		}
		/// <summary>
		/// 無効にする
		/// </summary>
		public void Disable() {
			enable_ = false;
		}

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Setup(Transform root, string name) {
            storage_ = new EntityStorage<T>(DefaultInstanceMax);
            //ゲームオブジェクトを作る
            cacheTrans_ = new GameObject(name).transform;
            cacheTrans_.SetParent(root);
            //Stock();
        }

		/// <summary>
		/// 初期化
		/// </summary>
		public virtual void Cleanup() {
			storage_.Clear();
		}
#if UNITY_EDITOR
		/// <summary>
		/// Entity生成
		/// </summary>
		/// <remarks>
		/// 直接Objectを使うバージョン(エディタで使用を想定
		/// </remarks>
		protected T CreateImpl(GameObject resObject, bool isImmediate, bool isReserve) {

            T entity = new T();
            GameObject obj = entity.Create(resObject);
            entity.SetOwn(obj);
            entity.SetParent(cacheTrans_);
			entity.ResName = "";
			entity.Setup();

			if (isImmediate) {
				storage_.AppendImmediate(entity);
			} else {
				storage_.Append(entity, isReserve);
			}
			return entity;
        }

#endif

		/// <summary>
		/// Entity生成
		/// </summary>
		protected async Task<T> CreateImplAsync(string resName, bool isImmediate, bool isReserve) {
			//使用可能なインスタンスが残っている場合
			T entity = storage_.Pop(resName);
			if (entity == null) {
				entity = await CreateNewEntityAsync(resName);
				if (IsStock) {
					storage_.EnableStock(resName);
				}
			}
			//インスタンスの使いまわしでも実行する初期化
			entity.Setup();
			if (isImmediate) {
				storage_.AppendImmediate(entity);
			} else {
				entity.SetActive(false);
				storage_.Append(entity, isReserve);
			}

			return entity;
		}
		/// <summary>
		/// 新規Entity生成
		/// </summary>
		/// <remarks>
		/// インスタンスが使いまわせずに新規生成するときに呼び出される
		/// </remarks>
		protected async Task<T> CreateNewEntityAsync(string resName) {
			T entity = new T();
			GameObject obj = await entity.CreateAsync(resName);
			entity.SetOwn(obj);
			entity.SetParent(cacheTrans_);
			entity.ResName = resName;

			return entity;
		}

        /// <summary>
        /// Entity破棄
        /// </summary>
        public void Destroy(T entity) {
			storage_.Remove(entity);
        }
        /// <summary>
        /// Entity破棄(まとめて)
        /// </summary>
        public void Destroy(List<T> entityList) {
            storage_.Remove(entityList);
        }

        /// <summary>
        /// 実行処理
        /// </summary>
        public void Execute() {
			if (!enable_)
				return;
            storage_.Execute();
        }
		/// <summary>
		/// 実行処理
		/// </summary>
		public void Evaluate() {
			if (!enable_)
				return;
			storage_.Evaluate();
		}
		/// <summary>
		/// 実行後処理
		/// </summary>
		public void LateExecute() {
			if (!enable_)
				return;
			storage_.LateExecute();
        }
		/// <summary>
		/// 管理下に書き出す
		/// </summary>
		public void Flush() {
			storage_.Flush();
		}

        //シングルトン
        private static U instance_;
        public static U I { get { return instance_; } }

        /// <summary>
        /// 明示的にインスタンスを作る
        /// </summary>
        public static IEntityAssembly CreateInstance(Transform root){
            Debug.Assert(instance_ == null, "already create instance");
            instance_ = new U();
            instance_.Setup(root, typeof(U).Name);
            return instance_;
        }

        /// <summary>
        /// 明示的にインスタンスを破棄
        /// </summary>
        public void DestroyInstance() {
			instance_.Cleanup();
			instance_ = null;
        }


    }
}
