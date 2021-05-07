using System;
using System.Collections;

namespace Characters.AI.Adventurer
{
	public abstract class SequenceSelector
	{
		public IEnumerator CRun(Strategy strategy)
		{
			if (strategy.role == Strategy.Role.Main)
			{
				switch (strategy.party)
				{
				case Strategy.Party.Solo:
					return CRunAsMainInSolo();
				case Strategy.Party.Duo:
					return CRunAsMainInDuo();
				case Strategy.Party.Trio:
					return CRunAsMainInTrio();
				}
			}
			else if (strategy.role == Strategy.Role.Sub)
			{
				switch (strategy.party)
				{
				case Strategy.Party.Solo:
					return CRunAsSubInSolo();
				case Strategy.Party.Duo:
					return CRunAsSubInDuo();
				case Strategy.Party.Trio:
					return CRunAsSubInTrio();
				}
			}
			throw new NotImplementedException();
		}

		protected abstract IEnumerator CRunAsMainInSolo();

		protected abstract IEnumerator CRunAsMainInDuo();

		protected abstract IEnumerator CRunAsMainInTrio();

		protected abstract IEnumerator CRunAsMainInQuatter();

		protected abstract IEnumerator CRunAsSubInSolo();

		protected abstract IEnumerator CRunAsSubInDuo();

		protected abstract IEnumerator CRunAsSubInTrio();

		protected abstract IEnumerator CRunAsSubInQuatter();
	}
}
