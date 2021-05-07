namespace BT.SharedValues
{
	public abstract class SharedValue
	{
	}
	public class SharedValue<T> : SharedValue
	{
		protected T _value;

		public T GetValue()
		{
			return _value;
		}

		public void SetValue(T value)
		{
			_value = value;
		}

		public SharedValue(T value)
		{
			_value = value;
		}
	}
}
