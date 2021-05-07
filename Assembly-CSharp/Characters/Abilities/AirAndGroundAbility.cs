using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities
{
	public class AirAndGroundAbility : AbilityAttacher
	{
		private enum Type
		{
			Ground,
			Air
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.movement.onJump += OnJump;
			base.owner.movement.onFall += OnFall;
			base.owner.movement.onGrounded += OnGrounded;
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.movement.onJump -= OnJump;
				base.owner.movement.onFall -= OnFall;
				base.owner.movement.onGrounded -= OnGrounded;
				Detach();
			}
		}

		private void OnJump(Movement.JumpType jumpType, float jumpHeight)
		{
			if (jumpType != Movement.JumpType.AirJump)
			{
				if (_type == Type.Ground)
				{
					Detach();
				}
				else
				{
					Attach();
				}
			}
		}

		private void OnFall()
		{
			Attach();
		}

		private void OnGrounded()
		{
			if (_type == Type.Ground)
			{
				Attach();
			}
			else
			{
				Detach();
			}
		}

		private void Attach()
		{
			base.owner.ability.Add(_abilityComponent.ability);
		}

		private void Detach()
		{
			base.owner.ability.Remove(_abilityComponent.ability);
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
