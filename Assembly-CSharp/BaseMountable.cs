#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BaseMountable : BaseCombatEntity
{
	public enum DismountConvarType
	{
		Misc = 0,
		Boating = 1,
		Flying = 2,
		GroundVehicle = 3,
		Horse = 4
	}

	public enum MountStatType
	{
		None = 0,
		Boating = 1,
		Flying = 2,
		Driving = 3
	}

	public enum MountGestureType
	{
		None = 0,
		UpperBody = 1
	}

	public static Translate.Phrase dismountPhrase = new Translate.Phrase("dismount", "Dismount");

	[Header("View")]
	public Transform eyePositionOverride;

	public Transform eyeCenterOverride;

	public Vector2 pitchClamp = new Vector2(-80f, 50f);

	public Vector2 yawClamp = new Vector2(-80f, 80f);

	public bool canWieldItems = true;

	public bool relativeViewAngles = true;

	[Header("Mounting")]
	public Transform mountAnchor;

	public float mountLOSVertOffset = 0.5f;

	public PlayerModel.MountPoses mountPose;

	public float maxMountDistance = 1.5f;

	public Transform[] dismountPositions;

	public bool checkPlayerLosOnMount;

	public bool disableMeshCullingForPlayers;

	public bool allowHeadLook;

	public bool ignoreVehicleParent;

	public bool legacyDismount;

	public ItemModWearable wearWhileMounted;

	public bool modifiesPlayerCollider;

	public BasePlayer.CapsuleColliderInfo customPlayerCollider;

	public SoundDefinition mountSoundDef;

	public SoundDefinition swapSoundDef;

	public SoundDefinition dismountSoundDef;

	public DismountConvarType dismountHoldType;

	public MountStatType mountTimeStatType;

	public MountGestureType allowedGestures;

	public bool canDrinkWhileMounted = true;

	public bool allowSleeperMounting;

	[Help("Set this to true if the mountable is enclosed so it doesn't move inside cars and such")]
	public bool animateClothInLocalSpace = true;

	[Header("Camera")]
	public BasePlayer.CameraMode MountedCameraMode;

	[Header("Rigidbody (Optional)")]
	public Rigidbody rigidBody;

	[FormerlySerializedAs("needsVehicleTick")]
	public bool isMobile;

	public float SideLeanAmount = 0.2f;

	public const float playerHeight = 1.8f;

	public const float playerRadius = 0.5f;

	public BasePlayer _mounted;

	public static ListHashSet<BaseMountable> FixedUpdateMountables = new ListHashSet<BaseMountable>();

	public override float PositionTickRate
	{
		protected get
		{
			return 0.05f;
		}
	}

	public virtual bool IsSummerDlcVehicle => false;

	public virtual bool BlocksDoors => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMountable.OnRpcMessage"))
		{
			if (rpc == 1735799362 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WantsDismount ");
				}
				using (TimeWarning.New("RPC_WantsDismount"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_WantsDismount(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_WantsDismount");
					}
				}
				return true;
			}
			if (rpc == 4014300952u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WantsMount ");
				}
				using (TimeWarning.New("RPC_WantsMount"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4014300952u, "RPC_WantsMount", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_WantsMount(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_WantsMount");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanHoldItems()
	{
		return canWieldItems;
	}

	public virtual BasePlayer.CameraMode GetMountedCameraMode()
	{
		return MountedCameraMode;
	}

	public virtual bool DirectlyMountable()
	{
		return true;
	}

	public virtual Transform GetEyeOverride()
	{
		if (eyePositionOverride != null)
		{
			return eyePositionOverride;
		}
		return base.transform;
	}

	public virtual bool ModifiesThirdPersonCamera()
	{
		return false;
	}

	public virtual Vector2 GetPitchClamp()
	{
		return pitchClamp;
	}

	public virtual Vector2 GetYawClamp()
	{
		return yawClamp;
	}

	public virtual bool AnyMounted()
	{
		return IsBusy();
	}

	public bool IsMounted()
	{
		return AnyMounted();
	}

	public virtual Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot)
	{
		if (player.GetMounted() != this)
		{
			return Vector3.zero;
		}
		return GetEyeOverride().position;
	}

	public virtual Vector3 EyeCenterForPlayer(BasePlayer player, Quaternion lookRot)
	{
		if (player.GetMounted() != this)
		{
			return Vector3.zero;
		}
		return eyeCenterOverride.transform.position;
	}

	public virtual float WaterFactorForPlayer(BasePlayer player)
	{
		return WaterLevel.Factor(player.WorldSpaceBounds().ToBounds(), waves: true, volumes: true, this);
	}

	public override float MaxVelocity()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((bool)baseEntity)
		{
			return baseEntity.MaxVelocity();
		}
		return base.MaxVelocity();
	}

	public virtual bool PlayerIsMounted(BasePlayer player)
	{
		if (BaseNetworkableEx.IsValid(player))
		{
			return player.GetMounted() == this;
		}
		return false;
	}

	public virtual BaseVehicle VehicleParent()
	{
		if (ignoreVehicleParent)
		{
			return null;
		}
		return GetParentEntity() as BaseVehicle;
	}

	public virtual bool HasValidDismountPosition(BasePlayer player)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			return baseVehicle.HasValidDismountPosition(player);
		}
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (ValidDismountPosition(player, transform.transform.position))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ValidDismountPosition(BasePlayer player, Vector3 disPos)
	{
		bool debugDismounts = Debugging.DebugDismounts;
		Vector3 dismountCheckStart = GetDismountCheckStart(player);
		if (debugDismounts)
		{
			Debug.Log($"ValidDismountPosition debug: Checking dismount point {disPos} from {dismountCheckStart}.");
		}
		Vector3 start = disPos + new Vector3(0f, 0.5f, 0f);
		Vector3 end = disPos + new Vector3(0f, 1.3f, 0f);
		if (!UnityEngine.Physics.CheckCapsule(start, end, 0.5f, 1537286401))
		{
			Vector3 position = disPos + base.transform.up * 0.5f;
			if (debugDismounts)
			{
				Debug.Log($"ValidDismountPosition debug: Dismount point {disPos} capsule check is OK.");
			}
			if (IsVisibleAndCanSee(position))
			{
				Vector3 vector = disPos + player.NoClipOffset();
				if (debugDismounts)
				{
					Debug.Log($"ValidDismountPosition debug: Dismount point {disPos} is visible.");
				}
				if (!AntiHack.TestNoClipping(dismountCheckStart, vector, player.NoClipRadius(ConVar.AntiHack.noclip_margin_dismount), ConVar.AntiHack.noclip_backtracking, sphereCast: true, out var _, vehicleLayer: false, legacyDismount ? null : this))
				{
					if (debugDismounts)
					{
						Debug.Log($"<color=green>ValidDismountPosition debug: Dismount point {disPos} is valid</color>.");
						Debug.DrawLine(dismountCheckStart, vector, Color.green, 10f);
					}
					return true;
				}
			}
		}
		if (debugDismounts)
		{
			Debug.DrawLine(dismountCheckStart, disPos, Color.red, 10f);
			if (debugDismounts)
			{
				Debug.Log($"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid</color>.");
			}
		}
		return false;
	}

	public BasePlayer GetMounted()
	{
		return _mounted;
	}

	public virtual void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
	}

	public virtual void LightToggle(BasePlayer player)
	{
	}

	public virtual void OnWeaponFired(BaseProjectile weapon)
	{
	}

	public virtual bool CanSwapToThis(BasePlayer player)
	{
		object obj = Interface.CallHook("CanSwapToSeat", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return !AnyMounted();
		}
		return false;
	}

	public override void OnKilled(HitInfo info)
	{
		DismountAllPlayers();
		base.OnKilled(info);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_WantsMount(RPCMessage msg)
	{
		WantsMount(msg.player);
	}

	public void WantsMount(BasePlayer player)
	{
		if (!BaseNetworkableEx.IsValid(player) || !player.CanInteract())
		{
			return;
		}
		BaseVehicle baseVehicle = default(BaseVehicle);
		if (!DirectlyMountable())
		{
			baseVehicle = VehicleParent();
			if (baseVehicle != null)
			{
				baseVehicle.WantsMount(player);
				return;
			}
		}
		if (Interface.CallHook("OnPlayerWantsMount", baseVehicle, this) == null)
		{
			AttemptMount(player);
		}
	}

	public virtual void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (_mounted != null || IsDead() || !player.CanMountMountablesNow() || IsTransferring())
		{
			return;
		}
		if (doMountChecks)
		{
			if (checkPlayerLosOnMount && UnityEngine.Physics.Linecast(player.eyes.position, mountAnchor.position + base.transform.up * mountLOSVertOffset, 1218652417))
			{
				Debug.Log("No line of sight to mount pos");
				return;
			}
			if (!HasValidDismountPosition(player))
			{
				Debug.Log("no valid dismount");
				return;
			}
		}
		MountPlayer(player);
	}

	public virtual bool AttemptDismount(BasePlayer player)
	{
		if (player != _mounted)
		{
			return false;
		}
		if (IsTransferring())
		{
			return false;
		}
		if (VehicleParent() != null && !VehicleParent().AllowPlayerInstigatedDismount(player))
		{
			return false;
		}
		DismountPlayer(player);
		return true;
	}

	[RPC_Server]
	public void RPC_WantsDismount(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!HasValidDismountPosition(player))
		{
			Interface.CallHook("OnPlayerDismountFailed", player, this);
		}
		else if (Interface.CallHook("OnPlayerWantsDismount", player, this) == null)
		{
			AttemptDismount(player);
		}
	}

	public void MountPlayer(BasePlayer player)
	{
		if (!(_mounted != null) && !(mountAnchor == null) && Interface.CallHook("CanMountEntity", player, this) == null)
		{
			player.EnsureDismounted();
			_mounted = player;
			Transform transform = mountAnchor.transform;
			player.MountObject(this);
			player.MovePosition(transform.position);
			player.transform.rotation = transform.rotation;
			player.ServerRotation = transform.rotation;
			player.OverrideViewAngles(transform.rotation.eulerAngles);
			_mounted.eyes.NetworkUpdate(transform.rotation);
			player.ClientRPCPlayer(null, player, "ForcePositionTo", player.transform.position);
			Facepunch.Rust.Analytics.Azure.OnMountEntity(player, this, VehicleParent());
			OnPlayerMounted();
			Interface.CallHook("OnEntityMounted", this, player);
		}
	}

	public virtual void OnPlayerMounted()
	{
		UpdateMountFlags();
	}

	public virtual void OnPlayerDismounted(BasePlayer player)
	{
		UpdateMountFlags();
	}

	public virtual void UpdateMountFlags()
	{
		SetFlag(Flags.Busy, _mounted != null);
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			baseVehicle.UpdateMountFlags();
		}
	}

	public virtual void DismountAllPlayers()
	{
		if ((bool)_mounted)
		{
			DismountPlayer(_mounted);
		}
	}

	public void DismountPlayer(BasePlayer player, bool lite = false)
	{
		if (_mounted == null || _mounted != player || Interface.CallHook("CanDismountEntity", player, this) != null)
		{
			return;
		}
		BaseVehicle baseVehicle = VehicleParent();
		Vector3 res;
		if (lite)
		{
			if (baseVehicle != null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			_mounted.DismountObject();
			_mounted = null;
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
			Interface.CallHook("OnEntityDismounted", this, player);
		}
		else if (!GetDismountPosition(player, out res) || Distance(res) > 10f)
		{
			if (baseVehicle != null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			res = player.transform.position;
			_mounted.DismountObject();
			_mounted.MovePosition(res);
			_mounted.ClientRPCPlayer(null, _mounted, "ForcePositionTo", res);
			BasePlayer mounted = _mounted;
			_mounted = null;
			Debug.LogWarning("Killing player due to invalid dismount point :" + player.displayName + " / " + player.userID + " on obj : " + base.gameObject.name);
			mounted.Hurt(1000f, DamageType.Suicide, mounted, useProtection: false);
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
		}
		else
		{
			if (baseVehicle != null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			_mounted.DismountObject();
			_mounted.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
			_mounted.MovePosition(res);
			_mounted.SendNetworkUpdateImmediate();
			_mounted.SendModelState(force: true);
			_mounted = null;
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			player.ForceUpdateTriggers();
			if ((bool)player.GetParentEntity())
			{
				BaseEntity baseEntity = player.GetParentEntity();
				player.ClientRPCPlayer(null, player, "ForcePositionToParentOffset", baseEntity.transform.InverseTransformPoint(res), baseEntity.net.ID);
			}
			else
			{
				player.ClientRPCPlayer(null, player, "ForcePositionTo", res);
			}
			Facepunch.Rust.Analytics.Azure.OnDismountEntity(player, this, baseVehicle);
			OnPlayerDismounted(player);
			Interface.CallHook("OnEntityDismounted", this, player);
		}
	}

	public virtual bool GetDismountPosition(BasePlayer player, out Vector3 res)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null && baseVehicle.IsVehicleMountPoint(this))
		{
			return baseVehicle.GetDismountPosition(player, out res);
		}
		int num = 0;
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (ValidDismountPosition(player, transform.transform.position))
			{
				res = transform.transform.position;
				return true;
			}
			num++;
		}
		Debug.LogWarning("Failed to find dismount position for player :" + player.displayName + " / " + player.userID + " on obj : " + base.gameObject.name);
		res = player.transform.position;
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (isMobile)
		{
			FixedUpdateMountables.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		FixedUpdateMountables.Remove(this);
		base.DoServerDestroy();
	}

	public static void FixedUpdateCycle()
	{
		for (int num = FixedUpdateMountables.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = FixedUpdateMountables[num];
			if (baseMountable == null)
			{
				FixedUpdateMountables.RemoveAt(num);
			}
			else if (baseMountable.isSpawned)
			{
				baseMountable.VehicleFixedUpdate();
			}
		}
		for (int num2 = FixedUpdateMountables.Count - 1; num2 >= 0; num2--)
		{
			BaseMountable baseMountable2 = FixedUpdateMountables[num2];
			if (baseMountable2 == null)
			{
				FixedUpdateMountables.RemoveAt(num2);
			}
			else if (baseMountable2.isSpawned)
			{
				baseMountable2.PostVehicleFixedUpdate();
			}
		}
	}

	public virtual void VehicleFixedUpdate()
	{
		if ((bool)_mounted)
		{
			_mounted.transform.rotation = mountAnchor.transform.rotation;
			_mounted.ServerRotation = mountAnchor.transform.rotation;
			_mounted.MovePosition(mountAnchor.transform.position);
		}
		if (!(rigidBody != null) || rigidBody.IsSleeping() || rigidBody.isKinematic)
		{
			return;
		}
		float num = ValidBounds.TestDist(base.transform.position) - 25f;
		if (num < 0f)
		{
			num = 0f;
		}
		if (!(num < 100f))
		{
			return;
		}
		Vector3 normalized = base.transform.position.normalized;
		float num2 = Vector3.Dot(rigidBody.velocity, normalized);
		if (num2 > 0f)
		{
			float num3 = 1f - num / 100f;
			rigidBody.velocity -= normalized * num2 * (num3 * num3);
			if (num < 25f)
			{
				float num4 = 1f - num / 25f;
				rigidBody.AddForce(-normalized * 20f * num4, ForceMode.Acceleration);
			}
		}
	}

	public virtual void PostVehicleFixedUpdate()
	{
	}

	public virtual void PlayerServerInput(InputState inputState, BasePlayer player)
	{
	}

	public virtual float GetComfort()
	{
		return 0f;
	}

	public virtual void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
	}

	public bool TryFireProjectile(StorageContainer ammoStorage, AmmoTypes ammoType, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		projectile = null;
		if (ammoStorage == null)
		{
			return false;
		}
		bool result = false;
		List<Item> obj = Facepunch.Pool.GetList<Item>();
		ammoStorage.inventory.FindAmmo(obj, ammoType);
		for (int num = obj.Count - 1; num >= 0; num--)
		{
			if (obj[num].amount <= 0)
			{
				obj.RemoveAt(num);
			}
		}
		if (obj.Count > 0)
		{
			if (UnityEngine.Physics.Raycast(firingPos, firingDir, out var hitInfo, launchOffset, 1237003025))
			{
				launchOffset = hitInfo.distance - 0.1f;
			}
			Item item = obj[obj.Count - 1];
			ItemModProjectile component = item.info.GetComponent<ItemModProjectile>();
			BaseEntity baseEntity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, firingPos + firingDir * launchOffset);
			projectile = baseEntity.GetComponent<ServerProjectile>();
			Vector3 vector = projectile.initialVelocity + firingDir * projectile.speed;
			if (minSpeed > 0f)
			{
				float num2 = Vector3.Dot(vector, firingDir) - minSpeed;
				if (num2 < 0f)
				{
					vector += firingDir * (0f - num2);
				}
			}
			projectile.InitializeVelocity(vector);
			if (BaseNetworkableEx.IsValid(shooter))
			{
				baseEntity.creatorEntity = shooter;
				baseEntity.OwnerID = shooter.userID;
			}
			baseEntity.Spawn();
			Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(shooter, baseEntity, this);
			item.UseItem();
			result = true;
		}
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public override void DisableTransferProtection()
	{
		base.DisableTransferProtection();
		BasePlayer mounted = GetMounted();
		if (mounted != null && mounted.IsTransferProtected())
		{
			mounted.DisableTransferProtection();
		}
	}

	public virtual bool IsInstrument()
	{
		return false;
	}

	public Vector3 GetDismountCheckStart(BasePlayer player)
	{
		Vector3 result = GetMountedPosition() + player.NoClipOffset();
		Vector3 vector = ((mountAnchor == null) ? base.transform.forward : mountAnchor.transform.forward);
		Vector3 vector2 = ((mountAnchor == null) ? base.transform.up : mountAnchor.transform.up);
		if (mountPose == PlayerModel.MountPoses.Chair)
		{
			result += -vector * 0.32f;
			result += vector2 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			result += -vector * 0.26f;
			result += vector2 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			result += -vector * 0.26f;
		}
		return result;
	}

	public Vector3 GetMountedPosition()
	{
		if (mountAnchor == null)
		{
			return base.transform.position;
		}
		return mountAnchor.transform.position;
	}

	public bool NearMountPoint(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (mountAnchor == null)
		{
			return false;
		}
		if (Vector3.Distance(player.transform.position, mountAnchor.position) <= maxMountDistance)
		{
			if (!UnityEngine.Physics.SphereCast(player.eyes.HeadRay(), 0.25f, out var hitInfo, 2f, 1218652417))
			{
				return false;
			}
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (entity != null)
			{
				if (entity == this || EqualNetID(entity))
				{
					return true;
				}
				if (entity is BasePlayer basePlayer)
				{
					BaseMountable mounted = basePlayer.GetMounted();
					if (mounted == this)
					{
						return true;
					}
					if (mounted != null && mounted.VehicleParent() == this)
					{
						return true;
					}
				}
				BaseEntity baseEntity = entity.GetParentEntity();
				if (RaycastHitEx.IsOnLayer(hitInfo, Rust.Layer.Vehicle_Detailed) && (baseEntity == this || EqualNetID(baseEntity)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Vector3 ConvertVector(Vector3 vec)
	{
		for (int i = 0; i < 3; i++)
		{
			if (vec[i] > 180f)
			{
				vec[i] -= 360f;
			}
			else if (vec[i] < -180f)
			{
				vec[i] += 360f;
			}
		}
		return vec;
	}
}
