using System.Collections;
using Characters;
using PhysicsUtils;
using UnityEngine;

namespace Runnables.Triggers
{
	public class EnterZone : Trigger
	{
		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private LayerMask _layer;

		private bool _result;

		private static readonly NonAllocOverlapper _lapper;

		static EnterZone()
		{
			_lapper = new NonAllocOverlapper(15);
		}

		private void Start()
		{
			StartCoroutine(CTriggerEnter());
		}

		public IEnumerator CTriggerEnter()
		{
			while (true)
			{
				_lapper.contactFilter.SetLayerMask(_layer);
				if (_lapper.OverlapCollider(_collider).GetComponent<Character>() != null)
				{
					_result = true;
				}
				if (_lapper.OverlapCollider(_collider).GetComponent<Character>() == null)
				{
					_result = false;
				}
				yield return null;
			}
		}

		protected override bool Check()
		{
			return _result;
		}
	}
}
