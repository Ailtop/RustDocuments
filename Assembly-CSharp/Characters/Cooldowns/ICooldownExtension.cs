namespace Characters.Cooldowns
{
	public static class ICooldownExtension
	{
		public static bool OnStreak(this ICooldown cooldown)
		{
			return cooldown.streak.remains < cooldown.streak.count;
		}
	}
}
