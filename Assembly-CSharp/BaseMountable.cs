#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BaseMountable : BaseCombatEntity
{
	public enum MountStatType
	{
		None,
		Boating,
		Flying,
		Driving
	}

	public enum MountGestureType
	{
		None,
		UpperBody
	}

	public BasePlayer _mounted;

	[Header("View")]
	public Transform eyeOverride;

	public Vector2 pitchClamp = new Vector2(-80f, 50f);

	public Vector2 yawClamp = new Vector2(-80f, 80f);

	public bool canWieldItems = true;

	public bool relativeViewAngles = true;

	[Header("Mounting")]
	public Transform mountAnchor;

	public PlayerModel.MountPoses mountPose;

	public float maxMountDistance = 1.5f;

	public Transform[] dismountPositions;

	public bool checkPlayerLosOnMount;

	public bool disableMeshCullingForPlayers;

	[SerializeField]
	private bool unparentOnMount = true;

	[FormerlySerializedAs("modifyPlayerCollider")]
	public bool modifiesPlayerCollider;

	public BasePlayer.CapsuleColliderInfo customPlayerCollider;

	public SoundDefinition mountSoundDef;

	public SoundDefinition swapSoundDef;

	public SoundDefinition dismountSoundDef;

	public MountStatType mountTimeStatType;

	public MountGestureType allowedGestures;

	public bool canDrinkWhileMounted = true;

	[Header("Camera")]
	public BasePlayer.CameraMode MountedCameraMode;

	public bool isMobile;

	public float SideLeanAmount = 0.2f;

	public static ListHashSet<BaseMountable> MobileMountables = new ListHashSet<BaseMountable>();

	public const float playerHeight = 1.8f;

	public const float playerRadius = 0.5f;

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
			RPCMessage rPCMessage;
			if (rpc == 1735799362 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_WantsDismount "));
				}
				using (TimeWarning.New("RPC_WantsDismount"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
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
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_WantsMount "));
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
							rPCMessage = default(RPCMessage);
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
		if (eyeOverride != null)
		{
			return eyeOverride;
		}
		return base.transform;
	}

	public virtual Quaternion GetMountedBodyAngles()
	{
		return GetEyeOverride().rotation;
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

	public virtual Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot)
	{
		if (player.GetMounted() != this)
		{
			return Vector3.zero;
		}
		return eyeOverride.transform.position;
	}

	public virtual float WaterFactorForPlayer(BasePlayer player)
	{
		return WaterLevel.Factor(player.WorldSpaceBounds().ToBounds(), this);
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

	public BasePlayer GetMounted()
	{
		return _mounted;
	}

	public BaseVehicle VehicleParent()
	{
		return GetParentEntity() as BaseVehicle;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetFlag(Flags.Busy, false);
	}

	public virtual void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
	}

	public virtual void LightToggle(BasePlayer player)
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

	public virtual bool IsMounted()
	{
		return _mounted != null;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return !IsMounted();
		}
		return false;
	}

	public override void OnKilled(HitInfo info)
	{
		DismountAllPlayers();
		base.OnKilled(info);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_WantsMount(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (DirectlyMountable() && Interface.CallHook("OnPlayerWantsMount", player, this) == null)
		{
			AttemptMount(player);
		}
	}

	public virtual void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (_mounted != null || IsDead() || !player.CanMountMountablesNow())
		{
			return;
		}
		if (doMountChecks)
		{
			if (checkPlayerLosOnMount && UnityEngine.Physics.Linecast(player.eyes.position, mountAnchor.position + base.transform.up * 0.5f, 1218652417))
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
		if (_mounted != null || mountAnchor == null || Interface.CallHook("CanMountEntity", player, this) != null)
		{
			return;
		}
		player.EnsureDismounted();
		_mounted = player;
		if (unparentOnMount)
		{
			TriggerParent triggerParent = player.FindTrigger<TriggerParent>();
			if ((bool)triggerParent)
			{
				triggerParent.OnTriggerExit(player.GetComponent<Collider>());
			}
		}
		player.MountObject(this);
		player.MovePosition(mountAnchor.transform.position);
		player.transform.rotation = mountAnchor.transform.rotation;
		player.ServerRotation = mountAnchor.transform.rotation;
		player.OverrideViewAngles(mountAnchor.transform.rotation.eulerAngles);
		_mounted.eyes.NetworkUpdate(mountAnchor.transform.rotation);
		player.ClientRPCPlayer(null, player, "ForcePositionTo", player.transform.position);
		SetFlag(Flags.Busy, true);
		OnPlayerMounted();
		Interface.CallHook("OnEntityMounted", this, player);
	}

	public virtual void OnPlayerMounted()
	{
	}

	public virtual void OnPlayerDismounted(BasePlayer player)
	{
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
			SetFlag(Flags.Busy, false);
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
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
			mounted.Hurt(1000f, DamageType.Suicide, mounted, false);
			SetFlag(Flags.Busy, false);
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
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
			_mounted = null;
			SetFlag(Flags.Busy, false);
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
			Interface.CallHook("OnEntityDismounted", this, player);
			OnPlayerDismounted(player);
		}
	}

	public bool ValidDismountPosition(Vector3 disPos, Vector3 visualCheckOrigin)
	{
		if (!UnityEngine.Physics.CheckCapsule(disPos + new Vector3(0f, 0.5f, 0f), disPos + new Vector3(0f, 1.3f, 0f), 0.5f, 1537286401))
		{
			Vector3 vector = disPos + base.transform.up * 0.5f;
			RaycastHit hitInfo;
			if (IsVisible(vector) && (!UnityEngine.Physics.Linecast(visualCheckOrigin, vector, out hitInfo, 1486946561) || _003CValidDismountPosition_003Eg__HitOurself_007C61_0(hitInfo)))
			{
				Ray ray = new Ray(visualCheckOrigin, Vector3Ex.Direction(vector, visualCheckOrigin));
				float maxDistance = Vector3.Distance(visualCheckOrigin, vector);
				if (!UnityEngine.Physics.SphereCast(ray, 0.5f, out hitInfo, maxDistance, 1486946561) || _003CValidDismountPosition_003Eg__HitOurself_007C61_0(hitInfo))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool HasValidDismountPosition(BasePlayer player)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			return baseVehicle.HasValidDismountPosition(player);
		}
		Vector3 visualCheckOrigin = player.TriggerPoint();
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (ValidDismountPosition(transform.transform.position, visualCheckOrigin))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool GetDismountPosition(BasePlayer player, out Vector3 res)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			return baseVehicle.GetDismountPosition(player, out res);
		}
		int num = 0;
		Vector3 visualCheckOrigin = player.TriggerPoint();
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (ValidDismountPosition(transform.transform.position, visualCheckOrigin))
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
			MobileMountables.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		MobileMountables.Remove(this);
		base.DoServerDestroy();
	}

	public static void FixedUpdateCycle()
	{
		for (int num = MobileMountables.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = MobileMountables[num];
			if (baseMountable == null)
			{
				MobileMountables.RemoveAt(num);
			}
			else
			{
				baseMountable.VehicleFixedUpdate();
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

	public virtual bool IsInstrument()
	{
		return false;
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
			RaycastHit hitInfo;
			if (!UnityEngine.Physics.SphereCast(player.eyes.HeadRay(), 0.25f, out hitInfo, 2f, 1218652417))
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
