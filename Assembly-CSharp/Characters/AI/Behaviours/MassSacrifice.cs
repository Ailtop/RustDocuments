using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class MassSacrifice : Behaviour
	{
		[SerializeField]
		private Action _action;

		[SerializeField]
		private Collider2D _range;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (!_action.TryStart())
			{
				base.result = Result.Fail;
				yield break;
			}
			while (_action.running)
			{
				yield return null;
			}
			base.result = Result.Success;
		}

		public bool CanUse(AIController aiController)
		{
			if (!_action.canUse)
			{
				return false;
			}
			_range.transform.position = aiController.target.transform.position;
			List<Character> list = aiController.FindEnemiesInRange(_range);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			foreach (Character item in list)
			{
				if (!(item.GetComponent<SacrificeCharacter>() == null))
				{
					return true;
				}
			}
			return false;
		}
	}
}
