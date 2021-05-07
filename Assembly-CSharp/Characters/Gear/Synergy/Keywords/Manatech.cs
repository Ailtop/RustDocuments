using System.Collections;
using Characters.Actions;
using Level;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Manatech : Keyword
	{
		[SerializeField]
		private DroppedManatechPart _manatechPart;

		[SerializeField]
		private float[] _countByLevel = new float[6] { 0f, 1f, 1f, 2f, 2f, 3f };

		public override Key key => Key.Manatech;

		protected override IList valuesByLevel => _countByLevel;

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
		}

		protected override void OnAttach()
		{
			base.character.onStartAction += OnStartAction;
		}

		protected override void OnDetach()
		{
			base.character.onStartAction -= OnStartAction;
		}

		private void OnStartAction(Action action)
		{
			if (action.type == Action.Type.Skill && !action.cooldown.usedByStreak)
			{
				for (int i = 0; (float)i < _countByLevel[base.level]; i++)
				{
					Vector3 position = base.transform.position;
					position.y += 0.5f;
					_manatechPart.poolObject.Spawn(position).GetComponent<DroppedManatechPart>().cooldownReducingAmount = 1f;
				}
			}
		}
	}
}
