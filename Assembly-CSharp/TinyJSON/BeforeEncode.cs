using System;

namespace TinyJSON;

[AttributeUsage(AttributeTargets.Method)]
public class BeforeEncode : Attribute
{
}
