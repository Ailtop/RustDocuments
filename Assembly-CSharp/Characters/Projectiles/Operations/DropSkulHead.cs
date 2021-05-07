namespace Characters.Projectiles.Operations
{
	public class DropSkulHead : Operation
	{
		private class Assets
		{
			internal static readonly PoolObject skulHead = Resource.instance.droppedSkulHead;
		}

		public override void Run(Projectile projectile)
		{
			Assets.skulHead.Spawn(base.transform.position);
		}
	}
}
