using System.Collections;

namespace Characters.AI.Adventurer
{
	public class MagicianSequenceSelector : SequenceSelector
	{
		private AdventurerMagician _controller;

		public MagicianSequenceSelector(AdventurerMagician magician)
		{
			_controller = magician;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (!(_controller.target == null) && !_controller.stuned)
			{
				if (_controller.CanUseWorldOnFire())
				{
					yield return _controller.RunPattern(new MagicianPattern.WorldOnFire());
					yield return _controller.RunPattern(new MagicianPattern.Idle());
				}
				if (_controller.CanUsePhoneixLanding())
				{
					yield return _controller.RunPattern(new MagicianPattern.PhoenixLanding());
					yield return _controller.RunPattern(new MagicianPattern.SkipableIdle());
				}
				else
				{
					yield return _controller.RunPattern(new MagicianPattern.KeepDistance());
					yield return _controller.RunPattern(new MagicianPattern.FireballCombo());
					yield return _controller.RunPattern(new MagicianPattern.SkipableIdle());
				}
			}
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			if (!(_controller.target == null) && !_controller.stuned)
			{
				if (_controller.CanUseWorldOnFire())
				{
					yield return _controller.RunPattern(new MagicianPattern.WorldOnFire());
					yield return _controller.RunPattern(new MagicianPattern.Idle());
				}
				if (_controller.CanUsePhoneixLandingLongCool())
				{
					yield return _controller.RunPattern(new MagicianPattern.PhoenixLandingLongCool());
					yield return _controller.RunPattern(new MagicianPattern.Idle());
				}
				else
				{
					yield return _controller.RunPattern(new MagicianPattern.KeepDistanceLongDistance());
					yield return _controller.RunPattern(_controller.magicianPatternSelector.GetNormalDualPattern());
					yield return _controller.RunPattern(new MagicianPattern.SkipableIdle());
				}
			}
		}

		protected override IEnumerator CRunAsMainInTrio()
		{
			return CRunAsMainInSolo();
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
