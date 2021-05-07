using System.Collections.Generic;

namespace Characters.AI.Adventurer
{
	public interface IRoleReplacementPolicy
	{
		void ReplaceMainAndSubs(ref Combat beforeMain, IList<Combat> subs);
	}
}
