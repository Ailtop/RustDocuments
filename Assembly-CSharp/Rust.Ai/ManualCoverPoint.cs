using UnityEngine;

namespace Rust.Ai;

public class ManualCoverPoint : FacepunchBehaviour
{
	public bool IsDynamic;

	public float Score = 2f;

	public CoverPointVolume Volume;

	public Vector3 Normal;

	public CoverPoint.CoverType NormalCoverType;

	public Vector3 Position => base.transform.position;

	public float DirectionMagnitude
	{
		get
		{
			if (Volume != null)
			{
				return Volume.CoverPointRayLength;
			}
			return 1f;
		}
	}

	private void Awake()
	{
		if (base.transform.parent != null)
		{
			Volume = base.transform.parent.GetComponent<CoverPointVolume>();
		}
	}

	public CoverPoint ToCoverPoint(CoverPointVolume volume)
	{
		Volume = volume;
		if (IsDynamic)
		{
			return new CoverPoint(Volume, Score)
			{
				IsDynamic = true,
				SourceTransform = base.transform,
				NormalCoverType = NormalCoverType,
				Position = (base.transform?.position ?? Vector3.zero)
			};
		}
		Vector3 normalized = (base.transform.rotation * Normal).normalized;
		return new CoverPoint(Volume, Score)
		{
			IsDynamic = false,
			Position = base.transform.position,
			Normal = normalized,
			NormalCoverType = NormalCoverType
		};
	}
}
