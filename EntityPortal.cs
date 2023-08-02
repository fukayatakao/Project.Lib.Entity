using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Lib;

namespace Project.Game {
	/// <summary>
	/// Entityへのアクセス経路
	/// </summary>
	/// <remarks>
	/// EntityはMonobehaviour継承しないのでrayなどで検出したgameObjectからEntityを拾うためのクラス
	/// </remarks>
	[DisallowMultipleComponent]
	public class EntityPortal<T> : MonoBehaviour where T : Entity {
        public T Owner;


        /// <summary>
        /// owner登録
        /// </summary>
        public void Init(T owner) {
            Owner = owner;
        }
    }
}
