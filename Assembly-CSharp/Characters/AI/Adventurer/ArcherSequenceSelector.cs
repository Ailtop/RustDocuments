using System.Collections;

namespace Characters.AI.Adventurer
{
	public class ArcherSequenceSelector : SequenceSelector
	{
		private AdventurerArcher _controller;

		public ArcherSequenceSelector(AdventurerArcher archer)
		{
			_controller = archer;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new ArcherPattern.ArrowRain());
				yield return _controller.RunPattern(new ArcherPattern.SkipableIdle());
			}
			if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUsePushAttack())
			{
				yield return _controller.RunPattern(new ArcherPattern.MeleeAttack());
				yield return _controller.RunPattern(new ArcherPattern.Attack());
			}
			else if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUseBackstep())
			{
				yield return _controller.RunPattern(new ArcherPattern.Backstep());
				if (_controller.CanUseExtraBackStep())
				{
					yield return _controller.RunPattern(new ArcherPattern.SecondBackstep());
				}
				yield return _controller.RunPattern(new ArcherPattern.Attack());
			}
			else
			{
				yield return _controller.RunPattern(_controller.archerPatternSelector.GetNormalSinglePattern());
			}
			yield return _controller.RunPattern(new ArcherPattern.SkipableIdle());
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new ArcherPattern.ArrowRain());
			}
			if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUseShortPushAttack())
			{
				yield return _controller.RunPattern(new ArcherPattern.MeleeAttackShortCool());
				yield return _controller.RunPattern(new ArcherPattern.Attack());
			}
			else if (_controller.IsTargetWithInMinimumDistance() && _controller.CanUseShortBackstep())
			{
				yield return _controller.RunPattern(new ArcherPattern.BackstepShortCool());
				if (_controller.CanUseExtraBackStep())
				{
					yield return _controller.RunPattern(new ArcherPattern.SecondBackstep());
				}
				yield return _controller.RunPattern(new ArcherPattern.Attack());
			}
			else
			{
				Pattern pattern = _controller.archerPatternSelector.GetNormalDualPattern();
				yield return _controller.RunPattern(pattern);
				if (pattern is ArcherPattern.BirdHunt)
				{
					yield return _controller.RunPattern(new ArcherPattern.Idle());
				}
				else
				{
					yield return _controller.RunPattern(new ArcherPattern.SkipableIdle());
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
