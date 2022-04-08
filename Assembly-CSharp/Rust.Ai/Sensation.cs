using UnityEngine;

namespace Rust.Ai;

public struct Sensation
{
	public SensationType Type;

	public Vector3 Position;

	public float Radius;

	public float DamagePotential;

	public BaseEntity Initiator;

	public BasePlayer InitiatorPlayer;

	public BaseEntity UsedEntity;
}
