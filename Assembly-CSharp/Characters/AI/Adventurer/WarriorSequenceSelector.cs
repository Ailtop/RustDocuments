using System.Collections;

namespace Characters.AI.Adventurer
{
	public class WarriorSequenceSelector : SequenceSelector
	{
		private AdventurerWarrior _controller;

		public WarriorSequenceSelector(AdventurerWarrior warrior)
		{
			_controller = warrior;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new WarriorPattern.Whirlwind());
				yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
			}
			Pattern pattern4;
			if (_controller.IsTargetWithInEarthQuakeTrigger())
			{
				pattern4 = _controller.warriorPatternSelector.GetOnEarthquakeTriggerStaySinglePattern();
				yield return _controller.RunPattern(pattern4);
				if (pattern4 is WarriorPattern.Stamping)
				{
					pattern4 = _controller.warriorPatternSelector.GetAfterStampingSinglePattern();
					yield return _controller.RunPattern(pattern4);
					if (pattern4 is WarriorPattern.Idle)
					{
						yield break;
					}
				}
				yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
				yield break;
			}
			pattern4 = _controller.warriorPatternSelector.GetNormalSinglePattern();
			yield return _controller.RunPattern(pattern4);
			if (pattern4 is WarriorPattern.Stamping)
			{
				pattern4 = _controller.warriorPatternSelector.GetAfterStampingSinglePattern();
				yield return _controller.RunPattern(pattern4);
				if (pattern4 is WarriorPattern.Idle)
				{
					yield break;
				}
			}
			yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new WarriorPattern.Whirlwind());
				yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
			}
			if (_controller.CanUseRescue())
			{
				yield return _controller.RunPattern(new WarriorPattern.Rescue());
				yield return _controller.RunPattern(new WarriorPattern.Guard());
				yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
			}
			Pattern pattern4;
			if (_controller.IsTargetWithInEarthQuakeTrigger())
			{
				pattern4 = _controller.warriorPatternSelector.GetOnEarthquakeTriggerStayDualPattern();
				yield return _controller.RunPattern(pattern4);
				if (pattern4 is WarriorPattern.Stamping)
				{
					pattern4 = _controller.warriorPatternSelector.GetAfterStampingDualPattern();
					yield return _controller.RunPattern(pattern4);
					if (pattern4 is WarriorPattern.Idle)
					{
						yield break;
					}
				}
				yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
				yield break;
			}
			pattern4 = _controller.warriorPatternSelector.GetNormalDualPattern();
			yield return _controller.RunPattern(pattern4);
			if (pattern4 is WarriorPattern.Stamping)
			{
				pattern4 = _controller.warriorPatternSelector.GetAfterStampingDualPattern();
				yield return _controller.RunPattern(pattern4);
				if (pattern4 is WarriorPattern.Idle)
				{
					yield break;
				}
			}
			yield return _controller.RunPattern(new WarriorPattern.SkipableIdle());
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
