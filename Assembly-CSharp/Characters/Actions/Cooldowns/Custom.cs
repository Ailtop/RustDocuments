namespace Characters.Actions.Cooldowns
{
	public class Custom : Cooldown
	{
		private bool _canUse;

		public override float remainPercent => (!_canUse) ? 1 : 0;

		public override bool canUse => _canUse;

		internal override bool Consume()
		{
			if (_canUse)
			{
				_canUse = false;
				return true;
			}
			return false;
		}

		internal void SetCanUse()
		{
			_canUse = true;
		}
	}
}
