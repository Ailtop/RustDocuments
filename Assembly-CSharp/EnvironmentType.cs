using System;

[Flags]
public enum EnvironmentType
{
	Underground = 0x1,
	Building = 0x2,
	Outdoor = 0x4,
	Elevator = 0x8,
	PlayerConstruction = 0x10,
	TrainTunnels = 0x20,
	UnderwaterLab = 0x40,
	Submarine = 0x80,
	BuildingDark = 0x100,
	BuildingVeryDark = 0x200
}
