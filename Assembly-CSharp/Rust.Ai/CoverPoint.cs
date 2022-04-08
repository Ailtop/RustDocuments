using System.Collections;
using UnityEngine;

namespace Rust.Ai;

public class CoverPoint
{
	public enum CoverType
	{
		Full = 0,
		Partial = 1,
		None = 2
	}

	public CoverType NormalCoverType;

	public bool IsDynamic;

	public Transform SourceTransform;

	private Vector3 _staticPosition;

	private Vector3 _staticNormal;

	public CoverPointVolume Volume { get; private set; }

	public Vector3 Position
	{
		get
		{
			if (IsDynamic && SourceTransform != null)
			{
				return SourceTransform.position;
			}
			return _staticPosition;
		}
		set
		{
			_staticPosition = value;
		}
	}

	public Vector3 Normal
	{
		get
		{
			if (IsDynamic && SourceTransform != null)
			{
				return SourceTransform.forward;
			}
			return _staticNormal;
		}
		set
		{
			_staticNormal = value;
		}
	}

	public BaseEntity ReservedFor { get; set; }

	public bool IsReserved => ReservedFor != null;

	public bool IsCompromised { get; set; }

	public float Score { get; set; }

	public bool IsValidFor(BaseEntity entity)
	{
		if (!IsCompromised)
		{
			if (!(ReservedFor == null))
			{
				return ReservedFor == entity;
			}
			return true;
		}
		return false;
	}

	public CoverPoint(CoverPointVolume volume, float score)
	{
		Volume = volume;
		Score = score;
	}

	public void CoverIsCompromised(float cooldown)
	{
		if (!IsCompromised && Volume != null)
		{
			Volume.StartCoroutine(StartCooldown(cooldown));
		}
	}

	private IEnumerator StartCooldown(float cooldown)
	{
		IsCompromised = true;
		yield return CoroutineEx.waitForSeconds(cooldown);
		IsCompromised = false;
	}

	public bool ProvidesCoverFromPoint(Vector3 point, float arcThreshold)
	{
		Vector3 normalized = (Position - point).normalized;
		return Vector3.Dot(Normal, normalized) < arcThreshold;
	}
}
