using System;
using System.Collections.Generic;
using UnityEngine;

public class Gibbable : PrefabAttribute, IClientComponent
{
	[Serializable]
	public struct OverrideMesh
	{
		public bool enabled;

		public ColliderType ColliderType;

		public Vector3 BoxSize;

		public Vector3 ColliderCentre;

		public float ColliderRadius;

		public float CapsuleHeight;

		public int CapsuleDirection;

		public bool BlockMaterialCopy;
	}

	public enum ColliderType
	{
		Box = 0,
		Sphere = 1,
		Capsule = 2
	}

	public enum ParentingType
	{
		None = 0,
		GibsOnly = 1,
		FXOnly = 2,
		All = 3
	}

	public enum BoundsEffectType
	{
		None = 0,
		Electrical = 1,
		Glass = 2,
		Scrap = 3,
		Stone = 4,
		Wood = 5
	}

	public GameObject gibSource;

	public Material[] customMaterials;

	public GameObject materialSource;

	public bool copyMaterialBlock = true;

	public bool applyDamageTexture;

	public PhysicMaterial physicsMaterial;

	public GameObjectRef fxPrefab;

	public bool spawnFxPrefab = true;

	[Tooltip("If enabled, gibs will spawn even though we've hit a gib limit")]
	public bool important;

	public bool useContinuousCollision;

	public float explodeScale;

	public float scaleOverride = 1f;

	[ReadOnly]
	public int uniqueId;

	public BoundsEffectType boundsEffectType;

	public bool isConditional;

	[ReadOnly]
	public Bounds effectBounds;

	public List<OverrideMesh> MeshOverrides = new List<OverrideMesh>();

	public bool UsePerGibWaterCheck;

	protected override Type GetIndexedType()
	{
		return typeof(Gibbable);
	}
}
