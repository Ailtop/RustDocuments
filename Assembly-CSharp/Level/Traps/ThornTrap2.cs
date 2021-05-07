using System.Collections;
using Characters;
using Characters.Actions;
using PhysicsUtils;
using UnityEngine;

namespace Level.Traps
{
	public class ThornTrap2 : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private CharacterAnimation _animation;

		[SerializeField]
		private AnimationClip _idleClip2;

		[SerializeField]
		private float _interval = 4f;

		[SerializeField]
		private Action _attackWithReadyAction;

		[SerializeField]
		private Action _attackAction;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private readonly NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		private void Awake()
		{
			_attackAction.Initialize(_character);
			_attackWithReadyAction.Initialize(_character);
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			do
			{
				yield return null;
				FindPlayer();
			}
			while (_overlapper.results.Count == 0);
			_animation.SetIdle(_idleClip2);
			_attackWithReadyAction.TryStart();
			yield return _attackWithReadyAction.CWaitForEndOfRunning();
			yield return Chronometer.global.WaitForSeconds(_interval);
			while (true)
			{
				yield return null;
				FindPlayer();
				if (_overlapper.results.Count != 0)
				{
					_attackAction.TryStart();
					yield return _attackAction.CWaitForEndOfRunning();
					yield return Chronometer.global.WaitForSeconds(_interval);
				}
			}
		}

		private void FindPlayer()
		{
			_range.enabled = true;
			_overlapper.contactFilter.SetLayerMask(512);
			_overlapper.OverlapCollider(_range);
			_range.enabled = false;
		}
	}
}
