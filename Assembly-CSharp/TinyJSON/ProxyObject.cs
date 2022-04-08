using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TinyJSON;

public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>, IEnumerable
{
	public const string TypeHintKey = "@type";

	private readonly Dictionary<string, Variant> dict;

	public string TypeHint
	{
		get
		{
			if (TryGetValue("@type", out var item))
			{
				return item.ToString(CultureInfo.InvariantCulture);
			}
			return null;
		}
	}

	public override Variant this[string key]
	{
		get
		{
			return dict[key];
		}
		set
		{
			dict[key] = value;
		}
	}

	public int Count => dict.Count;

	public Dictionary<string, Variant>.KeyCollection Keys => dict.Keys;

	public Dictionary<string, Variant>.ValueCollection Values => dict.Values;

	public ProxyObject()
	{
		dict = new Dictionary<string, Variant>();
	}

	IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator()
	{
		return dict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return dict.GetEnumerator();
	}

	public void Add(string key, Variant item)
	{
		dict.Add(key, item);
	}

	public bool TryGetValue(string key, out Variant item)
	{
		return dict.TryGetValue(key, out item);
	}
}
