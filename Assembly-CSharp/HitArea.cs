using System;

[Flags]
public enum HitArea
{
	Head = 0x1,
	Chest = 0x2,
	Stomach = 0x4,
	Arm = 0x8,
	Hand = 0x10,
	Leg = 0x20,
	Foot = 0x40
}
