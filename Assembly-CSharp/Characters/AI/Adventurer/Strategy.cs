using System.Collections.Generic;

namespace Characters.AI.Adventurer
{
	public class Strategy
	{
		public enum Party
		{
			Solo,
			Duo,
			Trio,
			Quattro
		}

		public enum Role
		{
			Main,
			Sub
		}

		public enum Position
		{
			None,
			Left,
			Right,
			Center
		}

		private Dictionary<int, Party> partyMapper;

		public Party party { get; private set; }

		public Role role { get; private set; }

		public Position position { get; private set; }

		public Strategy(int party, Role role, Position position)
		{
			partyMapper = new Dictionary<int, Party>(3)
			{
				{
					1,
					Party.Solo
				},
				{
					2,
					Party.Duo
				},
				{
					3,
					Party.Trio
				},
				{
					4,
					Party.Quattro
				}
			};
			this.party = (partyMapper.ContainsKey(party) ? partyMapper[party] : Party.Solo);
			this.role = role;
			this.position = position;
		}

		public Strategy(Party party, Role role, Position position)
		{
			this.party = party;
			this.role = role;
			this.position = position;
		}
	}
}
