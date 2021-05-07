using System.Collections.Generic;
using System.Linq;

namespace Characters.AI.Adventurer
{
	public class WeightedPatternSelector
	{
		private List<Pattern> _pool;

		public WeightedPatternSelector(Pattern[] patterns)
		{
			int capacity = patterns.Sum((Pattern x) => x.weight);
			_pool = new List<Pattern>(capacity);
			foreach (Pattern pattern in patterns)
			{
				for (int j = 0; j < pattern.weight; j++)
				{
					_pool.Add(pattern);
				}
			}
		}

		public Pattern GetPattern()
		{
			return _pool.Random();
		}
	}
}
