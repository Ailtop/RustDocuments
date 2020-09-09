using System;

[Flags]
public enum ItemSlot
{
	None = 0x1,
	Barrel = 0x2,
	Silencer = 0x4,
	Scope = 0x8,
	UnderBarrel = 0x10
}
