using System;
using System.Globalization;

namespace TinyJSON;

public sealed class ProxyNumber : Variant
{
	private static readonly char[] floatingPointCharacters = new char[2] { '.', 'e' };

	private readonly IConvertible value;

	public ProxyNumber(IConvertible value)
	{
		string text = value as string;
		this.value = ((text != null) ? Parse(text) : value);
	}

	private static IConvertible Parse(string value)
	{
		if (value.IndexOfAny(floatingPointCharacters) == -1)
		{
			ulong result2;
			if (value[0] == '-')
			{
				if (long.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result))
				{
					return result;
				}
			}
			else if (ulong.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result2))
			{
				return result2;
			}
		}
		if (decimal.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result3))
		{
			if (result3 == 0m && double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result4) && Math.Abs(result4) > double.Epsilon)
			{
				return result4;
			}
			return result3;
		}
		if (double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result5))
		{
			return result5;
		}
		return 0;
	}

	public override bool ToBoolean(IFormatProvider provider)
	{
		return value.ToBoolean(provider);
	}

	public override byte ToByte(IFormatProvider provider)
	{
		return value.ToByte(provider);
	}

	public override char ToChar(IFormatProvider provider)
	{
		return value.ToChar(provider);
	}

	public override decimal ToDecimal(IFormatProvider provider)
	{
		return value.ToDecimal(provider);
	}

	public override double ToDouble(IFormatProvider provider)
	{
		return value.ToDouble(provider);
	}

	public override short ToInt16(IFormatProvider provider)
	{
		return value.ToInt16(provider);
	}

	public override int ToInt32(IFormatProvider provider)
	{
		return value.ToInt32(provider);
	}

	public override long ToInt64(IFormatProvider provider)
	{
		return value.ToInt64(provider);
	}

	public override sbyte ToSByte(IFormatProvider provider)
	{
		return value.ToSByte(provider);
	}

	public override float ToSingle(IFormatProvider provider)
	{
		return value.ToSingle(provider);
	}

	public override string ToString(IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	public override ushort ToUInt16(IFormatProvider provider)
	{
		return value.ToUInt16(provider);
	}

	public override uint ToUInt32(IFormatProvider provider)
	{
		return value.ToUInt32(provider);
	}

	public override ulong ToUInt64(IFormatProvider provider)
	{
		return value.ToUInt64(provider);
	}
}
