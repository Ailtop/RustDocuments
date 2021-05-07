namespace Characters.Actions.Constraints.Customs
{
	public class Bullet
	{
		private int _maxCount;

		private int _currentCount;

		public Bullet(int maxCount)
		{
			_maxCount = maxCount;
			_currentCount = maxCount;
		}

		public bool Has(int amount)
		{
			if (_currentCount < amount)
			{
				return false;
			}
			return true;
		}

		public bool Consume(int amount)
		{
			if (!Has(amount))
			{
				return false;
			}
			_currentCount -= amount;
			return true;
		}

		public void Reload()
		{
			_currentCount = _maxCount;
		}
	}
}
