using System;

namespace TinyJSON;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DecodeAlias : Attribute
{
	public string[] Names { get; private set; }

	public DecodeAlias(params string[] names)
	{
		Names = names;
	}

	public bool Contains(string name)
	{
		return Array.IndexOf(Names, name) > -1;
	}
}
