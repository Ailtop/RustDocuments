namespace Characters
{
	public class TakeDamageEvent : PriorityList<TakeDamageDelegate>
	{
		public bool Invoke(ref Damage damage)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i].value(ref damage))
				{
					return true;
				}
			}
			return false;
		}
	}
}
