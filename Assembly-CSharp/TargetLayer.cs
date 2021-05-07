using System;
using UnityEngine;

[Serializable]
public class TargetLayer
{
	[SerializeField]
	private LayerMask _rawMask;

	[SerializeField]
	private bool _allyBody;

	[SerializeField]
	private bool _foeBody;

	[SerializeField]
	private bool _allyProjectile;

	[SerializeField]
	private bool _foeProjectile;

	public static bool IsPlayer(int layer)
	{
		return !IsMonster(layer);
	}

	public static bool IsMonster(int layer)
	{
		if (layer != 10)
		{
			return layer == 16;
		}
		return true;
	}

	public TargetLayer(LayerMask rawMask, bool allyBody, bool foeBody, bool allyProjectile, bool foeProjectile)
	{
		_rawMask = rawMask;
		_allyBody = allyBody;
		_foeBody = foeBody;
		_allyProjectile = allyProjectile;
		_foeProjectile = foeProjectile;
	}

	public LayerMask Evaluate(GameObject owner)
	{
		LayerMask layerMask = _rawMask;
		if (IsMonster(owner.layer))
		{
			if (_allyBody)
			{
				layerMask = (int)layerMask | 0x400;
			}
			if (_foeBody)
			{
				layerMask = (int)layerMask | 0x200;
			}
			if (_allyProjectile)
			{
				layerMask = (int)layerMask | 0x10000;
			}
			if (_foeProjectile)
			{
				layerMask = (int)layerMask | 0x8000;
			}
		}
		else
		{
			if (_allyBody)
			{
				layerMask = (int)layerMask | 0x200;
			}
			if (_foeBody)
			{
				layerMask = (int)layerMask | 0x400;
			}
			if (_allyProjectile)
			{
				layerMask = (int)layerMask | 0x8000;
			}
			if (_foeProjectile)
			{
				layerMask = (int)layerMask | 0x10000;
			}
		}
		return layerMask;
	}
}
