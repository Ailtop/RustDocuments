using Characters;
using Characters.Actions;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class IronDoor : ControlableTrap
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private Collider2D _blockCollider;

		[SerializeField]
		private Action _downAction;

		[SerializeField]
		private Action _upAction;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _hitOperations;

		private void Awake()
		{
			_hitOperations.Initialize();
			_character.health.onDied += Run;
			_character.gameObject.SetActive(false);
		}

		private void Run()
		{
			_character.health.onDied -= Run;
			StartCoroutine(_hitOperations.CRun(_character));
		}

		public override void Activate()
		{
			if (!base.activated)
			{
				_character.CancelAction();
				_character.gameObject.SetActive(true);
				_blockCollider.enabled = true;
				_downAction.TryStart();
				base.activated = true;
			}
		}

		public override void Deactivate()
		{
			if (base.activated && !_character.health.dead)
			{
				_character.CancelAction();
				_upAction.TryStart();
				_blockCollider.enabled = false;
				base.activated = false;
			}
		}
	}
}
