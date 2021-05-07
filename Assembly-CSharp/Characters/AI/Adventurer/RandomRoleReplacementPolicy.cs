using System.Collections.Generic;
using System.Linq;

namespace Characters.AI.Adventurer
{
	public class RandomRoleReplacementPolicy : IRoleReplacementPolicy
	{
		public void ReplaceMainAndSubs(ref Combat main, IList<Combat> subs)
		{
			if (subs.Count() > 0)
			{
				int index = subs.RandomIndex();
				if (main != null)
				{
					Combat combat = subs[index];
					subs[index] = main;
					main = combat;
				}
				else
				{
					main = subs[index];
					subs.RemoveAt(index);
				}
			}
		}
	}
}
