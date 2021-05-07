using System.Collections.Generic;

namespace Characters.AI.Adventurer
{
	public class Strategist
	{
		private static Strategist _instance;

		private List<Strategy> subStrategies;

		public static Strategist instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Strategist();
				}
				return _instance;
			}
		}

		private Strategist()
		{
			subStrategies = new List<Strategy>();
		}

		public Strategy GetMainStrategy(int alives)
		{
			return new Strategy(alives, Strategy.Role.Main, Strategy.Position.None);
		}

		public List<Strategy> GetSubStrategys(int alives)
		{
			subStrategies.Clear();
			for (int i = 0; i < alives; i++)
			{
				subStrategies.Add(new Strategy(alives, Strategy.Role.Sub, MMMaths.RandomBool() ? Strategy.Position.Left : Strategy.Position.Right));
			}
			return subStrategies;
		}
	}
}
