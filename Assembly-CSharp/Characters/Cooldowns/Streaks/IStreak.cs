namespace Characters.Cooldowns.Streaks
{
	public interface IStreak
	{
		int count { get; set; }

		float timeout { get; set; }

		int remains { get; }

		float remainPercent { get; }

		bool Consume();

		void Start();

		void Expire();
	}
}
