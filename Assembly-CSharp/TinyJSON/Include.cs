using System;

namespace TinyJSON;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Include : Attribute
{
}
