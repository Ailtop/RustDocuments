using Oxide.Core;
using Rust;
using UnityEngine;

public class CH47HelicopterAIController : CH47Helicopter
{
	public GameObjectRef scientistPrefab;

	public GameObjectRef dismountablePrefab;

	public GameObjectRef weakDismountablePrefab;

	public float maxTiltAngle = 0.3f;

	public float AiAltitudeForce = 10000f;

	public GameObjectRef lockedCratePrefab;

	public const Flags Flag_Damaged = Flags.Reserved7;

	public const Flags Flag_NearDeath = Flags.OnFire;

	public const Flags Flag_DropDoorOpen = Flags.Reserved8;

	public GameObject triggerHurt;

	public Vector3 landingTarget;

	public int numCrates = 1;

	private bool shouldLand;

	public bool aimDirOverride;

	public Vector3 _aimDirection = Vector3.forward;

	public Vector3 _moveTarget = Vector3.zero;

	public int lastAltitudeCheckFrame;

	public float altOverride;

	public float currentDesiredAltitude;

	private bool altitudeProtection = true;

	public float hoverHeight = 30f;

	public void DropCrate()
	{
		if (numCrates > 0)
		{
			Vector3 pos = base.transform.position + Vector3.down * 5f;
			Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			BaseEntity baseEntity = GameManager.server.CreateEntity(lockedCratePrefab.resourcePath, pos, rot);
			if ((bool)baseEntity)
			{
				Interface.CallHook("OnHelicopterDropCrate", this);
				baseEntity.SendMessage("SetWasDropped");
				baseEntity.Spawn();
			}
			numCrates--;
		}
	}

	public bool OutOfCrates()
	{
		object obj = Interface.CallHook("OnHelicopterOutOfCrates", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return numCrates <= 0;
	}

	public bool CanDropCrate()
	{
		object obj = Interface.CallHook("CanHelicopterDropCrate", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return numCrates > 0;
	}

	public bool IsDropDoorOpen()
	{
		return HasFlag(Flags.Reserved8);
	}

	public void SetDropDoorOpen(bool open)
	{
		if (Interface.CallHook("OnHelicopterDropDoorOpen", this) == null)
		{
			SetFlag(Flags.Reserved8, open);
		}
	}

	public bool ShouldLand()
	{
		return shouldLand;
	}

	public void SetLandingTarget(Vector3 target)
	{
		shouldLand = true;
		landingTarget = target;
		numCrates = 0;
	}

	public void ClearLandingTarget()
	{
		shouldLand = false;
	}

	public void TriggeredEventSpawn()
	{
		float x = TerrainMeta.Size.x;
		float y = 30f;
		Vector3 position = Vector3Ex.Range(-1f, 1f);
		position.y = 0f;
		position.Normalize();
		position *= x * 1f;
		position.y = y;
		base.transform.position = position;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (Interface.CallHook("CanUseHelicopter", player, this) == null)
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(SpawnScientists, 0.25f);
		SetMoveTarget(base.transform.position);
	}

	public void SpawnPassenger(Vector3 spawnPos, string prefabPath)
	{
		Quaternion identity = Quaternion.identity;
		HumanNPC component = GameManager.server.CreateEntity(prefabPath, spawnPos, identity).GetComponent<HumanNPC>();
		component.Spawn();
		AttemptMount(component);
	}

	public void SpawnPassenger(Vector3 spawnPos)
	{
		Quaternion identity = Quaternion.identity;
		HumanNPC component = GameManager.server.CreateEntity(dismountablePrefab.resourcePath, spawnPos, identity).GetComponent<HumanNPC>();
		component.Spawn();
		AttemptMount(component);
	}

	public void SpawnScientist(Vector3 spawnPos)
	{
		Quaternion identity = Quaternion.identity;
		HumanNPC component = GameManager.server.CreateEntity(scientistPrefab.resourcePath, spawnPos, identity).GetComponent<HumanNPC>();
		component.Spawn();
		AttemptMount(component);
		component.Brain.SetEnabled(false);
	}

	public void SpawnScientists()
	{
		if (shouldLand)
		{
			float dropoffScale = CH47LandingZone.GetClosest(landingTarget).dropoffScale;
			int num = Mathf.FloorToInt((float)(mountPoints.Count - 2) * dropoffScale);
			for (int i = 0; i < num; i++)
			{
				Vector3 spawnPos = base.transform.position + base.transform.forward * 10f;
				SpawnPassenger(spawnPos, dismountablePrefab.resourcePath);
			}
			for (int j = 0; j < 1; j++)
			{
				Vector3 spawnPos2 = base.transform.position - base.transform.forward * 15f;
				SpawnPassenger(spawnPos2);
			}
		}
		else
		{
			for (int k = 0; k < 4; k++)
			{
				Vector3 spawnPos3 = base.transform.position + base.transform.forward * 10f;
				SpawnScientist(spawnPos3);
			}
			for (int l = 0; l < 1; l++)
			{
				Vector3 spawnPos4 = base.transform.position - base.transform.forward * 15f;
				SpawnScientist(spawnPos4);
			}
		}
	}

	public void EnableFacingOverride(bool enabled)
	{
		aimDirOverride = enabled;
	}

	public void SetMoveTarget(Vector3 position)
	{
		_moveTarget = position;
	}

	public Vector3 GetMoveTarget()
	{
		return _moveTarget;
	}

	public void SetAimDirection(Vector3 dir)
	{
		_aimDirection = dir;
	}

	public Vector3 GetAimDirectionOverride()
	{
		return _aimDirection;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public override void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
		InitiateAnger();
	}

	public void CancelAnger()
	{
		if (base.SecondsSinceAttacked > 120f)
		{
			UnHostile();
			CancelInvoke(UnHostile);
		}
	}

	public void InitiateAnger()
	{
		CancelInvoke(UnHostile);
		Invoke(UnHostile, 120f);
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (!(mountPoint.mountable != null))
			{
				continue;
			}
			BasePlayer mounted = mountPoint.mountable.GetMounted();
			if ((bool)mounted)
			{
				ScientistNPC scientistNPC = mounted as ScientistNPC;
				if (scientistNPC != null)
				{
					scientistNPC.Brain.SetEnabled(true);
				}
			}
		}
	}

	public void UnHostile()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (!(mountPoint.mountable != null))
			{
				continue;
			}
			BasePlayer mounted = mountPoint.mountable.GetMounted();
			if ((bool)mounted)
			{
				ScientistNPC scientistNPC = mounted as ScientistNPC;
				if (scientistNPC != null)
				{
					scientistNPC.Brain.SetEnabled(false);
				}
			}
		}
	}

	public override void OnKilled(HitInfo info)
	{
		if (Interface.CallHook("OnEntityDestroy", this) == null)
		{
			if (!OutOfCrates())
			{
				DropCrate();
			}
			base.OnKilled(info);
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (Interface.CallHook("OnHelicopterAttacked", this, info) == null)
		{
			base.OnAttacked(info);
			InitiateAnger();
			SetFlag(Flags.Reserved7, base.healthFraction <= 0.8f);
			SetFlag(Flags.OnFire, base.healthFraction <= 0.33f);
		}
	}

	public void DelayedKill()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((bool)mounted && mounted.transform != null && !mounted.IsDestroyed && !mounted.IsDead() && mounted.IsNpc)
				{
					mounted.Kill();
				}
			}
		}
		Kill();
	}

	public override void DismountAllPlayers()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((bool)mounted)
				{
					mounted.Hurt(10000f, DamageType.Explosion, this, false);
				}
			}
		}
	}

	public void SetAltitudeProtection(bool on)
	{
		altitudeProtection = on;
	}

	public void CalculateDesiredAltitude()
	{
		CalculateOverrideAltitude();
		if (altOverride > currentDesiredAltitude)
		{
			currentDesiredAltitude = altOverride;
		}
		else
		{
			currentDesiredAltitude = Mathf.MoveTowards(currentDesiredAltitude, altOverride, Time.fixedDeltaTime * 5f);
		}
	}

	public void SetMinHoverHeight(float newHeight)
	{
		hoverHeight = newHeight;
	}

	public float CalculateOverrideAltitude()
	{
		if (Time.frameCount == lastAltitudeCheckFrame)
		{
			return altOverride;
		}
		lastAltitudeCheckFrame = Time.frameCount;
		float y = GetMoveTarget().y;
		float num = Mathf.Max(TerrainMeta.WaterMap.GetHeight(GetMoveTarget()), TerrainMeta.HeightMap.GetHeight(GetMoveTarget()));
		float num2 = Mathf.Max(y, num + hoverHeight);
		if (altitudeProtection)
		{
			Vector3 rhs = ((rigidBody.velocity.magnitude < 0.1f) ? base.transform.forward : rigidBody.velocity.normalized);
			Vector3 normalized = (Vector3.Cross(Vector3.Cross(base.transform.up, rhs), Vector3.up) + Vector3.down * 0.3f).normalized;
			RaycastHit hitInfo;
			RaycastHit hitInfo2;
			if (Physics.SphereCast(base.transform.position - normalized * 20f, 20f, normalized, out hitInfo, 75f, 1218511105) && Physics.SphereCast(hitInfo.point + Vector3.up * 200f, 20f, Vector3.down, out hitInfo2, 200f, 1218511105))
			{
				num2 = hitInfo2.point.y + hoverHeight;
			}
		}
		altOverride = num2;
		return altOverride;
	}

	public override void SetDefaultInputState()
	{
		currentInputState.Reset();
		Vector3 moveTarget = GetMoveTarget();
		Vector3 vector = Vector3.Cross(base.transform.right, Vector3.up);
		Vector3 vector2 = Vector3.Cross(Vector3.up, vector);
		float num = 0f - Vector3.Dot(Vector3.up, base.transform.right);
		float num2 = Vector3.Dot(Vector3.up, base.transform.forward);
		float num3 = Vector3Ex.Distance2D(base.transform.position, moveTarget);
		float y = base.transform.position.y;
		float num4 = currentDesiredAltitude;
		Vector3 vector3 = base.transform.position + base.transform.forward * 10f;
		vector3.y = num4;
		Vector3 lhs = Vector3Ex.Direction2D(moveTarget, base.transform.position);
		float num5 = 0f - Vector3.Dot(lhs, vector2);
		float num6 = Vector3.Dot(lhs, vector);
		float num7 = Mathf.InverseLerp(0f, 25f, num3);
		if (num6 > 0f)
		{
			float num8 = Mathf.InverseLerp(0f - maxTiltAngle, 0f, num2);
			currentInputState.pitch = 1f * num6 * num8 * num7;
		}
		else
		{
			float num9 = 1f - Mathf.InverseLerp(0f, maxTiltAngle, num2);
			currentInputState.pitch = 1f * num6 * num9 * num7;
		}
		if (num5 > 0f)
		{
			float num10 = Mathf.InverseLerp(0f - maxTiltAngle, 0f, num);
			currentInputState.roll = 1f * num5 * num10 * num7;
		}
		else
		{
			float num11 = 1f - Mathf.InverseLerp(0f, maxTiltAngle, num);
			currentInputState.roll = 1f * num5 * num11 * num7;
		}
		float value = Mathf.Abs(num4 - y);
		float num12 = 1f - Mathf.InverseLerp(10f, 30f, value);
		currentInputState.pitch *= num12;
		currentInputState.roll *= num12;
		float num13 = maxTiltAngle;
		float num14 = Mathf.InverseLerp(0f + Mathf.Abs(currentInputState.pitch) * num13, num13 + Mathf.Abs(currentInputState.pitch) * num13, Mathf.Abs(num2));
		currentInputState.pitch += num14 * ((num2 < 0f) ? (-1f) : 1f);
		float num15 = Mathf.InverseLerp(0f + Mathf.Abs(currentInputState.roll) * num13, num13 + Mathf.Abs(currentInputState.roll) * num13, Mathf.Abs(num));
		currentInputState.roll += num15 * ((num < 0f) ? (-1f) : 1f);
		if (aimDirOverride || num3 > 30f)
		{
			Vector3 rhs = (aimDirOverride ? GetAimDirectionOverride() : Vector3Ex.Direction2D(GetMoveTarget(), base.transform.position));
			Vector3 to = (aimDirOverride ? GetAimDirectionOverride() : Vector3Ex.Direction2D(GetMoveTarget(), base.transform.position));
			float num16 = Vector3.Dot(vector2, rhs);
			float f = Vector3.Angle(vector, to);
			float num17 = Mathf.InverseLerp(0f, 70f, Mathf.Abs(f));
			currentInputState.yaw = ((num16 > 0f) ? 1f : 0f);
			currentInputState.yaw -= ((num16 < 0f) ? 1f : 0f);
			currentInputState.yaw *= num17;
		}
		float throttle = Mathf.InverseLerp(5f, 30f, num3);
		currentInputState.throttle = throttle;
	}

	public void MaintainAIAltutide()
	{
		Vector3 vector = base.transform.position + rigidBody.velocity;
		float num = currentDesiredAltitude;
		float y = vector.y;
		float value = Mathf.Abs(num - y);
		bool flag = num > y;
		float num2 = Mathf.InverseLerp(0f, 10f, value) * AiAltitudeForce * (flag ? 1f : (-1f));
		rigidBody.AddForce(Vector3.up * num2, ForceMode.Force);
	}

	public override void VehicleFixedUpdate()
	{
		hoverForceScale = 1f;
		base.VehicleFixedUpdate();
		SetFlag(Flags.Reserved5, TOD_Sky.Instance.IsNight);
		CalculateDesiredAltitude();
		MaintainAIAltutide();
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			foreach (MountPointInfo mountPoint in mountPoints)
			{
				if (mountPoint.mountable != null)
				{
					BasePlayer mounted = mountPoint.mountable.GetMounted();
					if ((bool)mounted && mounted.transform != null && !mounted.IsDestroyed && !mounted.IsDead() && mounted.IsNpc)
					{
						mounted.Kill();
					}
				}
			}
		}
		base.DestroyShared();
	}
}
