using ConVar;

namespace Facepunch.Rust
{
	public static class Analytics
	{
		internal static void Death(string v)
		{
			bool official = Server.official;
		}

		public static void Crafting(string targetItemShortname, int taskSkinId)
		{
			bool official = Server.official;
		}

		public static void ExcavatorStarted()
		{
			if (Server.official)
			{
				GA.DesignEvent("excavatorstarted");
			}
		}

		public static void ExcavatorStopped(float activeDuration)
		{
			if (Server.official)
			{
				GA.DesignEvent("excavatorstopped", activeDuration);
			}
		}

		public static void SlotMachineTransaction(int scrapSpent, int scrapReceived)
		{
			if (Server.official)
			{
				GA.DesignEvent("slotsScrapSpent", scrapSpent);
				GA.DesignEvent("slotsScrapReceived", scrapReceived);
			}
		}
	}
}
