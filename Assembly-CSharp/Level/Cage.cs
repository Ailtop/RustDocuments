using System;
using Characters;
using UnityEngine;

namespace Level
{
	public class Cage : MonoBehaviour
	{
		[SerializeField]
		private Target _target;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private SpriteRenderer _behind;

		[SerializeField]
		private Sprite _behindWreckage;

		[SerializeField]
		private Prop _prop;

		public Collider2D collider => _collider;

		public event Action onDestroyed;

		private void Awake()
		{
			_prop.onDestroy += Destroy;
		}

		public void Activate()
		{
			collider.enabled = true;
		}

		public void Deactivate()
		{
			collider.enabled = false;
		}

		public void Destroy()
		{
			_collider.enabled = false;
			_behind.sprite = _behindWreckage;
			this.onDestroyed?.Invoke();
			Deactivate();
		}
	}
}
