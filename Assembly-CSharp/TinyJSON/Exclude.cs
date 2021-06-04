using System;

namespace TinyJSON
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Exclude : Attribute
	{
	}
}
