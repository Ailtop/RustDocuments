using System;

[Flags]
public enum EntityType
{
	Player = 0x1,
	NPC = 0x2,
	WorldItem = 0x4,
	Corpse = 0x8,
	TimedExplosive = 0x10,
	Chair = 0x20,
	BasePlayerNPC = 0x40
}
