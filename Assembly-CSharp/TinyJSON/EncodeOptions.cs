using System;

namespace TinyJSON
{
	[Flags]
	public enum EncodeOptions
	{
		None = 0x0,
		PrettyPrint = 0x1,
		NoTypeHints = 0x2,
		IncludePublicProperties = 0x4,
		EnforceHierarchyOrder = 0x8,
		[Obsolete("Use EncodeOptions.EnforceHierarchyOrder instead.")]
		EnforceHeirarchyOrder = 0x8
	}
}
