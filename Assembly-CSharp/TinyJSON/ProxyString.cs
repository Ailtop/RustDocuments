using System;

namespace TinyJSON;

public sealed class ProxyString : Variant
{
	private readonly string value;

	public ProxyString(string value)
	{
		this.value = value;
	}

	public override string ToString(IFormatProvider provider)
	{
		return value;
	}
}
