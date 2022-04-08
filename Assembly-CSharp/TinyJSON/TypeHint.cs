using System;

namespace TinyJSON;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TypeHint : Attribute
{
}
