using System.Collections;
using Characters;
using PhysicsUtils;
using UnityEngine;

namespace Level.MapEvent.Condition
{
	public class EnterZone : Condition
	{
		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private LayerMask _layer;

		[SerializeField]
		private bool _once = true;

		private static readonly NonAllocOverlapper _lapper;

		static EnterZone()
		{
			_lapper = new NonAllocOverlapper(15);
		}

		private bool IsCharacterStayingZone()
		{
			_lapper.contactFilter.SetLayerMask(_layer);
			return _lapper.OverlapCollider(_collider).GetComponent<Character>() != null;
		}

		private void Awake()
		{
			StartCoroutine(CRunOnTriggerEnter());
		}

		private IEnumerator CRunOnTriggerEnter()
		{
			yield return null;
			while (true)
			{
				if (IsCharacterStayingZone())
				{
					Run();
					if (_once)
					{
						break;
					}
					yield return CWaitTriggerExit();
				}
				yield return null;
			}
		}

		private IEnumerator CWaitTriggerExit()
		{
			yield return null;
			while (IsCharacterStayingZone())
			{
				yield return null;
			}
		}
	}
}
