using System.Collections.Generic;

namespace Characters.Controllers
{
	public class Context
	{
		private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

		public T Get<T>(string name)
		{
			return (T)_dictionary[name];
		}

		public void Set<T>(string name, T value)
		{
			if (_dictionary.ContainsKey(name))
			{
				_dictionary[name] = value;
			}
			else
			{
				_dictionary.Add(name, value);
			}
		}

		public ContextVariable<T> GetVariable<T>(string name)
		{
			return new ContextVariable<T>(this, name);
		}
	}
	public class Context<TKey>
	{
		private readonly Dictionary<TKey, object> _dictionary = new Dictionary<TKey, object>();

		public TVal Get<TVal>(TKey key)
		{
			return (TVal)_dictionary[key];
		}

		public void Set<TVal>(TKey key, TVal value)
		{
			if (_dictionary.ContainsKey(key))
			{
				_dictionary[key] = value;
			}
			else
			{
				_dictionary.Add(key, value);
			}
		}

		public ContextVariable<TKey, TVal> GetVariable<TVal>(TKey key)
		{
			return new ContextVariable<TKey, TVal>(this, key);
		}
	}
}
