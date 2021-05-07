using System.Collections;

namespace Characters.AI.Adventurer
{
	public class ClericSequenceSelector : SequenceSelector
	{
		private AdventurerCleric _controller;

		public ClericSequenceSelector(AdventurerCleric cleric)
		{
			_controller = cleric;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new ClericPattern.MassiveHeal());
				yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				yield break;
			}
			if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUseEscapeTeleport())
			{
				yield return _controller.RunPattern(new ClericPattern.EscapeTeleport());
				yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				yield break;
			}
			if (_controller.CanUseSelfHeal())
			{
				yield return _controller.RunPattern(new ClericPattern.Heal());
				yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				yield break;
			}
			for (int i = 0; i < 3; i++)
			{
				yield return _controller.RunPattern(new ClericPattern.HolyCross());
			}
			yield return _controller.RunPattern(new ClericPattern.Idle());
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			if (!(_controller.target == null) && !_controller.stuned)
			{
				if (_controller.CanUseCastingSkill())
				{
					yield return _controller.RunPattern(new ClericPattern.MassiveHeal());
					yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				}
				else if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUseEscapeTeleport())
				{
					yield return _controller.RunPattern(new ClericPattern.EscapeTeleportLongCool());
					yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				}
				else if (_controller.CanUseHealShortCool())
				{
					yield return _controller.RunPattern(new ClericPattern.HealShortCool());
					yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				}
				else
				{
					yield return _controller.RunPattern(_controller.clericPatternSelector.GetNormalDualPattern());
					yield return _controller.RunPattern(new ClericPattern.SkipableIdle());
				}
			}
		}

		protected override IEnumerator CRunAsMainInTrio()
		{
			return CRunAsMainInDuo();
		}

		protected override IEnumerator CRunAsMainInQuatter()
		{
			return CRunAsMainInDuo();
		}

		protected override IEnumerator CRunAsSubInSolo()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsSubInDuo()
		{
			return CRunAsMainInDuo();
		}

		protected override IEnumerator CRunAsSubInTrio()
		{
			return CRunAsMainInDuo();
		}

		protected override IEnumerator CRunAsSubInQuatter()
		{
			return CRunAsMainInDuo();
		}
	}
}
