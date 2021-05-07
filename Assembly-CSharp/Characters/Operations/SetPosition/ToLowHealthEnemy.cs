using System.Collections.Generic;
using Characters.AI;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToLowHealthEnemy : Policy
	{
		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private Collider2D _findRange;

		[SerializeField]
		private bool _includeSelf = true;

		public override Vector2 GetPosition()
		{
			Character character = GetLowHealthCharacter();
			if (character == null)
			{
				character = _controller.character;
			}
			return character.transform.position;
		}

		private Character GetLowHealthCharacter()
		{
			List<Character> list = _controller.FindEnemiesInRange(_findRange);
			double num = 1.0;
			Character result = null;
			foreach (Character item in list)
			{
				if (item.liveAndActive && (_includeSelf || !(item == _controller.character)) && item.health.percent < num)
				{
					num = item.health.percent;
					result = item;
				}
			}
			return result;
		}
	}
}
