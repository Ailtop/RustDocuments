namespace Characters
{
	public class GiveDamageEvent : PriorityList<GiveDamageDelegate>
	{
		public bool Invoke(ITarget target, ref Damage damage)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i].value(target, ref damage))
				{
					return true;
				}
			}
			return false;
		}
	}
}
