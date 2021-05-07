namespace Level.Traps
{
	public abstract class ControlableTrap : Trap
	{
		public bool activated { get; protected set; }

		public abstract void Activate();

		public abstract void Deactivate();
	}
}
