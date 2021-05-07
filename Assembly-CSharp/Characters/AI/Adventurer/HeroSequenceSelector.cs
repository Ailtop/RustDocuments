using System.Collections;

namespace Characters.AI.Adventurer
{
	public class HeroSequenceSelector : SequenceSelector
	{
		private AdventurerHero _controller;

		public HeroSequenceSelector(AdventurerHero hero)
		{
			_controller = hero;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new HeroPattern.SwordAuraWave());
				yield return _controller.RunPattern(new HeroPattern.SkipableIdle());
			}
			if (_controller.IsTargetWithInDashTrigger() && _controller.CanUseBackDash())
			{
				yield return _controller.RunPattern(new HeroPattern.BackDash());
				Pattern pattern2 = _controller.heroPatternSelector.GetAfterBackDashSinglePattern();
				yield return _controller.RunPattern(pattern2);
				if (pattern2 is HeroPattern.EnergyBall)
				{
					yield return _controller.RunPattern(new HeroPattern.SkipableIdle());
				}
			}
			else if (_controller.IsTargetWithInDashTrigger())
			{
				yield return _controller.RunPattern(_controller.heroPatternSelector.GetNormalSinglePattern());
				yield return _controller.RunPattern(new HeroPattern.SkipableIdle());
			}
			else
			{
				Pattern pattern2 = _controller.heroPatternSelector.GetOutDashSinglePattern();
				yield return _controller.RunPattern(pattern2);
				if (pattern2 is HeroPattern.Dash)
				{
					yield return _controller.RunPattern(_controller.heroPatternSelector.GetAfterDashSinglePattern());
				}
				yield return _controller.RunPattern(new HeroPattern.SkipableIdle());
			}
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsMainInTrio()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsMainInQuatter()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsSubInSolo()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsSubInDuo()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsSubInTrio()
		{
			return CRunAsMainInSolo();
		}

		protected override IEnumerator CRunAsSubInQuatter()
		{
			return CRunAsMainInSolo();
		}
	}
}
