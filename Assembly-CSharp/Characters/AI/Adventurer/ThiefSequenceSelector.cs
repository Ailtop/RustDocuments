using System.Collections;

namespace Characters.AI.Adventurer
{
	public class ThiefSequenceSelector : SequenceSelector
	{
		private AdventurerThief _controller;

		public ThiefSequenceSelector(AdventurerThief thief)
		{
			_controller = thief;
		}

		protected override IEnumerator CRunAsMainInSolo()
		{
			if (_controller.target == null || _controller.stuned)
			{
				yield break;
			}
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new ThiefPattern.MultipleBunshin());
				yield return _controller.RunPattern(new ThiefPattern.SkipableIdle());
			}
			if (_controller.CanUseGiganticShuriken())
			{
				yield return _controller.RunPattern(new ThiefPattern.GiganticShuriken());
				yield return _controller.RunPattern(new ThiefPattern.SkipableIdle());
				yield break;
			}
			yield return _controller.RunPattern(new ThiefPattern.ShadowStep());
			Pattern pattern2 = _controller.thiefPatternSelector.GetAfterShadowSinglePattern();
			yield return _controller.RunPattern(pattern2);
			if (pattern2 is ThiefPattern.FlashCut)
			{
				pattern2 = _controller.thiefPatternSelector.GetAfterFlashCutSinglePattern();
				yield return _controller.RunPattern(pattern2);
				if (pattern2 is ThiefPattern.Idle)
				{
					yield break;
				}
			}
			yield return _controller.RunPattern(new ThiefPattern.SkipableIdle());
		}

		protected override IEnumerator CRunAsMainInDuo()
		{
			if (_controller.CanUseCastingSkill())
			{
				yield return _controller.RunPattern(new ThiefPattern.MultipleBunshin());
			}
			if (_controller.CanUseGiganticShurikenLongCool())
			{
				yield return _controller.RunPattern(new ThiefPattern.GiganticShurikenLongCool());
				yield return _controller.RunPattern(new ThiefPattern.SkipableIdle());
				yield break;
			}
			yield return _controller.RunPattern(new ThiefPattern.ShadowStep());
			Pattern pattern2 = _controller.thiefPatternSelector.GetAfterShadowDualPattern();
			yield return _controller.RunPattern(pattern2);
			if (pattern2 is ThiefPattern.FlashCut)
			{
				pattern2 = _controller.thiefPatternSelector.GetAfterFlashCutDualPattern();
				yield return _controller.RunPattern(pattern2);
				if (pattern2 is ThiefPattern.Idle)
				{
					yield break;
				}
			}
			yield return _controller.RunPattern(new ThiefPattern.SkipableIdle());
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
