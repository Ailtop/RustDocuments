using System;

namespace TinyJSON;

public sealed class DecodeException : Exception
{
	public DecodeException(string message)
		: base(message)
	{
	}

	public DecodeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
