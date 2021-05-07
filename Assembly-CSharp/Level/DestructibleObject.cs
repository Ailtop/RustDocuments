using System;
using Characters;
using UnityEngine;

namespace Level
{
	public abstract class DestructibleObject : MonoBehaviour
	{
		protected Action _onDestroy;

		[SerializeField]
		private bool _blockCast;

		[SerializeField]
		private bool _spawnEffectOnHit = true;

		public abstract Collider2D collider { get; }

		public bool blockCast => _blockCast;

		public bool spawnEffectOnHit => _spawnEffectOnHit;

		public bool destroyed { get; protected set; }

		public event Action onDestroy
		{
			add
			{
				_onDestroy = (Action)Delegate.Combine(_onDestroy, value);
			}
			remove
			{
				_onDestroy = (Action)Delegate.Remove(_onDestroy, value);
			}
		}

		public void Hit(Character from, ref Damage damage)
		{
			Hit(from, ref damage, Vector2.zero);
		}

		public abstract void Hit(Character from, ref Damage damage, Vector2 force);
	}
}
