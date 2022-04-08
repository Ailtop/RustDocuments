using System;

namespace TinyJSON;

public sealed class ProxyBoolean : Variant
{
	private readonly bool value;

	public ProxyBoolean(bool value)
	{
		this.value = value;
	}

	public override bool ToBoolean(IFormatProvider provider)
	{
		return value;
	}

	public override string ToString(IFormatProvider provider)
	{
		if (!value)
		{
			return "false";
		}
		return "true";
	}
}
