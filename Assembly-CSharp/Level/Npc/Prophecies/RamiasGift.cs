namespace Level.Npc.Prophecies
{
	public class RamiasGift : Prophecy
	{
		public RamiasGift(string key, Rarity rarity)
			: base(key, rarity)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		protected override void GetReward()
		{
			DropGear();
		}

		protected override void OnActivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		protected override void Reset()
		{
		}
	}
}
