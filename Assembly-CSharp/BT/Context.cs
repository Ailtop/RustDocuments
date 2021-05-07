using System.Collections.Generic;
using BT.SharedValues;

namespace BT
{
	public class Context : Dictionary<string, SharedValue>
	{
		public float deltaTime { get; set; }

		public T Get<T>(string name)
		{
			SharedValue value;
			if (TryGetValue(name, out value))
			{
				return (value as SharedValue<T>).GetValue();
			}
			return default(T);
		}

		public void Set<T>(string name, SharedValue<T> value)
		{
			if (!ContainsKey(name))
			{
				Add(name, value);
			}
			else
			{
				base[name] = value;
			}
		}

		public static Context Create()
		{
			return new Context();
		}
	}
}
