using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Lib {

	/// <summary>
	/// インスタンス管理クラス
	/// </summary>
	public class EntityStorage<T> where T : Entity {
        //有効な配列末尾Index=有効なインスタンス数
		public int TailIndex;
        //インスタンス配列
		public T[] Current;
		private List<T> memory_;
		private List<T> reserve_;

		//デフォルトの最大インスタンス数
		private int max_ = 128;

        // インスタンス保持用
        private Dictionary<string, Stack<T>> stock_ = new Dictionary<string, Stack<T>>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public EntityStorage(int max){
			TailIndex = 0;
			max_ = max;
			Current = new T[max_];
			memory_ = new List<T>();
			reserve_ = new List<T>();
		}
		/// <summary>
		/// 実行処理
		/// </summary>
		public void Execute() {
            for (int i = 0; i < TailIndex; i++) {
                Current[i].Execute();
            }
        }
		/// <summary>
		/// Playableの更新
		/// </summary>
		public void Evaluate() {
			for (int i = 0; i < TailIndex; i++) {
				Current[i].Evaluate();
			}
		}
		/// <summary>
		/// 実行後処理
		/// </summary>
		public void LateExecute() {
            for (int i = 0; i < TailIndex; i++) {
                Current[i].LateExecute();
                if (!Current[i].IsExist()) {
                    //高速化のためPopで消えた場所に配列の最後の要素を持ってきて入れてるのでi--でもう一度同じindexの要素を実行する必要がある
                    Remove(ref i);
                }
            }
        }
        /// <summary>
        /// 使っているインスタンスとストック両方を全削除
        /// </summary>
        public void Clear() {
			for (int i = 0; i < TailIndex; i++) {
				Current[i].Cleanup();
				Current[i].Destroy();
				Current[i].Release();
				Current[i] = null;
			}
			for (int i = 0, max = memory_.Count; i < max; i++) {
				memory_[i].Cleanup();
				memory_[i].Destroy();
				memory_[i].Release();
			}
			memory_.Clear();
			for (int i = 0, max = reserve_.Count; i < max; i++) {
				reserve_[i].Cleanup();
				reserve_[i].Destroy();
				reserve_[i].Release();
			}
			reserve_.Clear();
			//stockではCleanup済なので呼ばない
			foreach (Stack<T> stack in stock_.Values){
				foreach (T t in stack) {
					t.Destroy();
					t.Release();
				}
			}
			stock_.Clear();
			TailIndex = 0;
		}
		/// <summary>
		/// ストック場所を作る
		/// </summary>
		public void EnableStock(string resName) {
			if (!stock_.ContainsKey(resName)) {
				stock_[resName] = new Stack<T>();
			}
		}
		/// <summary>
		/// ストック場所を廃棄する
		/// </summary>
		public void DisableStock(string resName) {
			if (stock_.ContainsKey(resName)) {
				stock_.Remove(resName);
			}
		}

		/// <summary>
		/// ストックに入れる
		/// </summary>
		public void Push(T entity, string resName) {
			entity.SetActive(false);
			if (!stock_.ContainsKey(resName)) {
				stock_[resName] = new Stack<T>();
			}
			stock_[resName].Push(entity);
		}
		/// <summary>
		/// ストックから取り出す
		/// </summary>
		public T Pop(string resName) {
			if (stock_.ContainsKey(resName) && stock_[resName].Count > 0)
				return stock_[resName].Pop();
			else
				return null;
		}
		/// <summary>
		/// 直接追加に追加
		/// </summary>
		public void AppendImmediate(T entity) {
			AppendImpl(entity, true);
		}
		/// <summary>
		/// 管理に加える
		/// </summary>
		private void AppendImpl(T entity, bool isActive) {
			//インスタンス数が制限数を上回った場合
			if (TailIndex >= Current.Length) {
				//とりあえず警告出しとく
				Debug.LogWarning("over instance count");
				//配列のサイズを増やす
				Array.Resize(ref Current, Current.Length * 2);
			}
			Current[TailIndex] = entity;
			TailIndex++;
			entity.SetActive(isActive);
		}

		/// <summary>
		/// 一時領域に追加
		/// </summary>
		public void Append(T entity, bool isReserve) {
			if (!isReserve) {
				memory_.Add(entity);
			} else {
				reserve_.Add(entity);
			}
		}
		/// <summary>
		/// 一時領域に蓄えたインスタンスを管理領域に書き出す
		/// </summary>
		public void Flush() {
			for(int i = 0, max = memory_.Count; i < max; i++) {
				AppendImpl(memory_[i], true);
			}
			for (int i = 0, max = reserve_.Count; i < max; i++) {
				AppendImpl(reserve_[i], false);
			}
			memory_.Clear();
			reserve_.Clear();
		}
		/// <summary>
		/// 管理から取り除く
		/// </summary>
		public void Remove(T entity){
			for (int i = 0; i < TailIndex; i++) {
				//該当のインスタンスが見つかったら
				if(Current [i] == entity){
                    //取り除く
                    Remove(ref i);
					break;
				}
			}
		}
        /// <summary>
        /// 管理から取り除く(まとめて)
        /// </summary>
        public void Remove(List<T> entityList){
			for (int i = 0; i < TailIndex; i++) {
				for (int j = 0, max = entityList.Count; j < max; j++) {
					//該当のインスタンスが見つかったら
					if (Current[i] == entityList[j]) {
                        //取り除く
                        Remove(ref i);
						break;
					}
				}
			}
		}
		/// <summary>
		/// 取り除く(実処理)
		/// </summary>
		private void Remove(ref int index) {
			T entity = Current[index];
			entity.SetActive(false);
			//インスタンスを使いまわすかどうかに関わらず実行する終了処理
			entity.Cleanup();
			//ストックする場所が用意されている場合はストックに戻す
			if (stock_.ContainsKey(entity.ResName)) {
				stock_[entity.ResName].Push(entity);
			//ストックする場所がない場合はそのまま破棄
			} else {
				entity.Destroy();
				entity.Release();
			}
			//空いた場所に一番最後のインスタンスを入れてリストの長さを減らす
			Current[index] = Current[TailIndex - 1];
			Current[TailIndex - 1] = null;
			TailIndex--;
			index--;
		}

	}
}
