using System.Collections.Generic;
using Facepunch.BurstCloth;
using UnityEngine;

public class BurstClothHitBoxCollision : BurstCloth, IClientComponent, IPrefabPreProcess
{
	[Header("Rust Wearable BurstCloth")]
	public float GravityStrength = 0.8f;

	public float DefaultLength = 1f;

	public float MountedLengthMultiplier;

	public float DuckedLengthMultiplier = 0.5f;

	public float CorpseLengthMultiplier = 0.2f;

	[Header("Collision")]
	public string[] IgnoreKeywords;

	protected override void GatherColliders(List<CapsuleParams> colliders)
	{
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
