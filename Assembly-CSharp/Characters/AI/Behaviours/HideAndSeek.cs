using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class HideAndSeek : Behaviour
	{
		[SerializeField]
		private Action _attackReady;

		public override IEnumerator CRun(AIController controller)
		{
			_003C_003Ec__DisplayClass1_0 _003C_003Ec__DisplayClass1_ = default(_003C_003Ec__DisplayClass1_0);
			_003C_003Ec__DisplayClass1_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass1_.character = controller.character;
			_003C_003Ec__DisplayClass1_.target = controller.target;
			if (_003C_003Ec__DisplayClass1_.target == null)
			{
				base.result = Result.Fail;
				yield break;
			}
			_003C_003Ec__DisplayClass1_.targetCollisionState = _003C_003Ec__DisplayClass1_.target.movement.controller.collisionState;
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				if (_003C_003Ec__DisplayClass1_.target == null)
				{
					base.result = Result.Fail;
					break;
				}
				if (!_003CCRun_003Eg__CanChase_007C1_0(ref _003C_003Ec__DisplayClass1_))
				{
					yield return null;
				}
				else if ((bool)controller.FindClosestPlayerBody(controller.stopTrigger))
				{
					if (!_attackReady.TryStart())
					{
						yield return null;
						continue;
					}
					while (_attackReady.running && base.result == Result.Doing)
					{
						yield return null;
						if (!_003CCRun_003Eg__CanChase_007C1_0(ref _003C_003Ec__DisplayClass1_))
						{
							_003C_003Ec__DisplayClass1_.character.CancelAction();
							base.result = Result.Fail;
							break;
						}
					}
					if (base.result != 0)
					{
						base.result = Result.Success;
						break;
					}
					base.result = Result.Doing;
				}
				else
				{
					float num = controller.target.transform.position.x - _003C_003Ec__DisplayClass1_.character.transform.position.x;
					_003C_003Ec__DisplayClass1_.character.movement.move = ((num > 0f) ? Vector2.right : Vector2.left);
					yield return null;
				}
			}
		}

		private bool isFacingEachOther(Character character, Character target)
		{
			character.ForceToLookAt(target.transform.position.x);
			if (character.lookingDirection == Character.LookingDirection.Right && target.lookingDirection == Character.LookingDirection.Left)
			{
				return true;
			}
			if (character.lookingDirection == Character.LookingDirection.Left && target.lookingDirection == Character.LookingDirection.Right)
			{
				return true;
			}
			return false;
		}
	}
}
