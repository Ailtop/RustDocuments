using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Thief
{
	public class MultipleBunshin : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Teleport))]
		private Teleport _teleport;

		[SerializeField]
		private Action _castringReady;

		[SerializeField]
		private float _term;

		[SerializeField]
		private Transform _teleportDestination;

		[SerializeField]
		private Transform _startPosition;

		[SerializeField]
		private OperationInfos _flashCutBunshin;

		[SerializeField]
		private OperationInfos _shurikenBunshin;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			_teleportDestination.position = new Vector2(platformBounds.center.x, platformBounds.max.y);
			yield return _teleport.CRun(controller);
			_castringReady.TryStart();
			while (_castringReady.running)
			{
				yield return null;
			}
			StartCoroutine(_castingSkill.CRun(controller));
			while (_castingSkill.result == Result.Doing)
			{
				if (MMMaths.RandomBool())
				{
					SetFlashCutStartPosition(platformBounds);
					_flashCutBunshin.gameObject.SetActive(true);
					_flashCutBunshin.Run(character);
				}
				else
				{
					SetShurikenStartPosition(platformBounds);
					_shurikenBunshin.gameObject.SetActive(true);
					_shurikenBunshin.Run(character);
				}
				yield return Chronometer.global.WaitForSeconds(_term);
			}
			if (_castingSkill.result == Result.Fail)
			{
				base.result = Result.Fail;
			}
			else
			{
				base.result = Result.Success;
			}
		}

		private void SetFlashCutStartPosition(Bounds bounds)
		{
			if (MMMaths.RandomBool())
			{
				_startPosition.position = new Vector2(Random.Range(bounds.min.x + 1f, bounds.center.x), bounds.max.y);
			}
			else
			{
				_startPosition.position = new Vector2(Random.Range(bounds.center.x, bounds.max.x - 1f), bounds.max.y);
			}
		}

		private void SetShurikenStartPosition(Bounds bounds)
		{
			if (MMMaths.RandomBool())
			{
				_startPosition.position = new Vector2(bounds.min.x + 1f, bounds.max.y);
			}
			else
			{
				_startPosition.position = new Vector2(bounds.max.x - 1f, bounds.max.y);
			}
		}

		public bool CanUse()
		{
			return _castingSkill.CanUse();
		}
	}
}
