#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseFishingRod : HeldEntity
{
	public enum CatchState
	{
		None,
		Aiming,
		Waiting,
		Catching,
		Caught
	}

	[Flags]
	public enum FishState
	{
		PullingLeft = 0x1,
		PullingRight = 0x2,
		PullingBack = 0x4
	}

	public enum FailReason
	{
		UserRequested,
		BadAngle,
		TensionBreak,
		Unequipped,
		TimeOut,
		Success,
		NoWaterFound,
		Obstructed,
		NoLure,
		TooShallow,
		TooClose,
		TooFarAway,
		PlayerMoved
	}

	private FishLookup fishLookup;

	private TimeUntil nextFishStateChange;

	private TimeSince fishCatchDuration;

	private float strainTimer;

	private const float strainMax = 6f;

	private TimeSince lastStrainUpdate;

	private TimeUntil catchTime;

	private TimeSince lastSightCheck;

	private Vector3 playerStartPosition;

	private WaterBody surfaceBody;

	private ItemDefinition lureUsed;

	private ItemDefinition currentFishTarget;

	private ItemModFishable fishableModifier;

	private ItemModFishable lastFish;

	[ServerVar(Saved = true)]
	public static bool ForceSuccess = false;

	[ServerVar(Saved = true)]
	public static bool ForceFail = false;

	[ServerVar(Saved = true)]
	public static bool ImmediateHook = false;

	public GameObjectRef FishingBobberRef;

	public float FishCatchDistance = 0.5f;

	public LineRenderer ReelLineRenderer;

	public Transform LineRendererWorldStartPos;

	private FishState currentFishState;

	private EntityRef<FishingBobber> currentBobber;

	public float ConditionLossOnSuccess = 0.02f;

	public float ConditionLossOnFail = 0.04f;

	public float GlobalStrainSpeedMultiplier = 1f;

	public float MaxCastDistance = 10f;

	public const Flags Straining = Flags.Reserved1;

	public ItemModFishable ForceFish;

	public static Flags ReelingInFlag = Flags.Reserved8;

	public GameObjectRef BobberPreview;

	public SoundDefinition onLineSoundDef;

	public SoundDefinition strainSoundDef;

	public AnimationCurve strainGainCurve;

	public SoundDefinition tensionBreakSoundDef;

	public CatchState CurrentState { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseFishingRod.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 4237324865u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_Cancel "));
				}
				using (TimeWarning.New("Server_Cancel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4237324865u, "Server_Cancel", this, player))
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
							RPCMessage msg2 = rPCMessage;
							Server_Cancel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_Cancel");
					}
				}
				return true;
			}
			if (rpc == 4238539495u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_RequestCast "));
				}
				using (TimeWarning.New("Server_RequestCast"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4238539495u, "Server_RequestCast", this, player))
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
							Server_RequestCast(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_RequestCast");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Server_RequestCast(RPCMessage msg)
	{
		Vector3 pos = msg.read.Vector3();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		Item currentLure = GetCurrentLure();
		if (currentLure == null)
		{
			FailedCast(FailReason.NoLure);
			return;
		}
		FailReason reason;
		if (!EvaluateFishingPosition(ref pos, ownerPlayer, out reason, out surfaceBody))
		{
			FailedCast(reason);
			return;
		}
		FishingBobber component = base.gameManager.CreateEntity(FishingBobberRef.resourcePath, base.transform.position + Vector3.up * 2.8f + ownerPlayer.eyes.BodyForward() * 1.8f, GetOwnerPlayer().ServerRotation).GetComponent<FishingBobber>();
		component.transform.forward = GetOwnerPlayer().eyes.BodyForward();
		component.Spawn();
		component.InitialiseBobber(ownerPlayer, surfaceBody, pos);
		lureUsed = currentLure.info;
		currentLure.UseItem();
		if (fishLookup == null)
		{
			fishLookup = PrefabAttribute.server.Find<FishLookup>(prefabID);
		}
		currentFishTarget = fishLookup.GetFish(component.transform.position, surfaceBody, lureUsed, out fishableModifier, lastFish);
		lastFish = fishableModifier;
		currentBobber.Set(component);
		ClientRPC(null, "Client_ReceiveCastPoint", component.net.ID);
		ownerPlayer.SignalBroadcast(Signal.Attack);
		catchTime = (ImmediateHook ? 0f : UnityEngine.Random.Range(10f, 20f));
		catchTime = (float)catchTime * fishableModifier.CatchWaitTimeMultiplier;
		ItemModCompostable component2;
		float val = (lureUsed.TryGetComponent<ItemModCompostable>(out component2) ? component2.BaitValue : 0f);
		val = Mathx.RemapValClamped(val, 0f, 20f, 1f, 10f);
		catchTime = Mathf.Clamp((float)catchTime - val, 3f, 20f);
		playerStartPosition = ownerPlayer.transform.position;
		SetFlag(Flags.Busy, true);
		CurrentState = CatchState.Waiting;
		InvokeRepeating(CatchProcess, 0f, 0f);
	}

	private void FailedCast(FailReason reason)
	{
		CurrentState = CatchState.None;
		ClientRPC(null, "Client_ResetLine", (int)reason);
	}

	private void CatchProcess()
	{
		FishingBobber fishingBobber = currentBobber.Get(true);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null || ownerPlayer.IsSleeping() || ownerPlayer.IsWounded() || ownerPlayer.IsDead())
		{
			Server_Cancel(FailReason.UserRequested);
			return;
		}
		Vector3 position = ownerPlayer.transform.position;
		float num = Vector3.Angle((fishingBobber.transform.position.WithY(0f) - position.WithY(0f)).normalized, ownerPlayer.eyes.HeadForward().WithY(0f));
		float num2 = Vector3.Distance(position, fishingBobber.transform.position.WithY(position.y));
		if (num > ((num2 > 1.2f) ? 60f : 180f))
		{
			Server_Cancel(FailReason.BadAngle);
			return;
		}
		if (num2 > 1.2f && (float)lastSightCheck > 0.4f)
		{
			if (!GamePhysics.LineOfSight(ownerPlayer.eyes.position, fishingBobber.transform.position, 1218511105))
			{
				Server_Cancel(FailReason.Obstructed);
				return;
			}
			lastSightCheck = 0f;
		}
		if (Vector3.Distance(position, fishingBobber.transform.position) > MaxCastDistance * 2f)
		{
			Server_Cancel(FailReason.TooFarAway);
			return;
		}
		if (Vector3.Distance(playerStartPosition, position) > 1f)
		{
			Server_Cancel(FailReason.PlayerMoved);
			return;
		}
		if (CurrentState == CatchState.Waiting)
		{
			if ((float)catchTime < 0f)
			{
				ClientRPC(null, "Client_HookedSomething");
				CurrentState = CatchState.Catching;
				fishingBobber.SetFlag(Flags.Reserved1, true);
				nextFishStateChange = 0f;
				fishCatchDuration = 0f;
				strainTimer = 0f;
			}
			return;
		}
		FishState fishState = currentFishState;
		if ((float)nextFishStateChange < 0f)
		{
			float num3 = Mathx.RemapValClamped(fishingBobber.TireAmount, 0f, 20f, 0f, 1f);
			if (currentFishState != 0)
			{
				currentFishState = (FishState)0;
				nextFishStateChange = UnityEngine.Random.Range(2f, 4f) * (num3 + 1f);
			}
			else
			{
				nextFishStateChange = UnityEngine.Random.Range(3f, 7f) * (1f - num3);
				if (UnityEngine.Random.Range(0, 100) < 50)
				{
					currentFishState = FishState.PullingLeft;
				}
				else
				{
					currentFishState = FishState.PullingRight;
				}
				if (UnityEngine.Random.Range(0, 100) > 60 && Vector3.Distance(fishingBobber.transform.position, ownerPlayer.transform.position) < MaxCastDistance - 2f)
				{
					currentFishState |= FishState.PullingBack;
				}
			}
		}
		if ((float)fishCatchDuration > 120f)
		{
			Server_Cancel(FailReason.TimeOut);
			return;
		}
		bool flag = ownerPlayer.serverInput.IsDown(BUTTON.RIGHT);
		bool flag2 = ownerPlayer.serverInput.IsDown(BUTTON.LEFT);
		bool flag3 = HasReelInInput(ownerPlayer.serverInput);
		if (flag2 && flag)
		{
			flag2 = (flag = false);
		}
		UpdateFlags(flag2, flag, flag3);
		if (CurrentState == CatchState.Waiting)
		{
			flag = (flag2 = (flag3 = false));
		}
		if (flag2 && !AllowPullInDirection(-ownerPlayer.eyes.HeadRight(), fishingBobber.transform.position))
		{
			flag2 = false;
		}
		if (flag && !AllowPullInDirection(ownerPlayer.eyes.HeadRight(), fishingBobber.transform.position))
		{
			flag = false;
		}
		fishingBobber.ServerMovementUpdate(flag2, flag, flag3, ref currentFishState, position, fishableModifier);
		bool flag4 = false;
		float num4 = 0f;
		if (flag3 || flag2 || flag)
		{
			flag4 = true;
			num4 = 0.5f;
		}
		if (currentFishState != 0 && flag4)
		{
			if (FishStateExtensions.Contains(currentFishState, FishState.PullingBack) && flag3)
			{
				num4 = 1.5f;
			}
			else if ((FishStateExtensions.Contains(currentFishState, FishState.PullingLeft) || FishStateExtensions.Contains(currentFishState, FishState.PullingRight)) && flag3)
			{
				num4 = 1.2f;
			}
			else if (FishStateExtensions.Contains(currentFishState, FishState.PullingLeft) && flag)
			{
				num4 = 0.8f;
			}
			else if (FishStateExtensions.Contains(currentFishState, FishState.PullingRight) && flag2)
			{
				num4 = 0.8f;
			}
		}
		if (flag3 && currentFishState != 0)
		{
			num4 += 1f;
		}
		num4 *= fishableModifier.StrainModifier * GlobalStrainSpeedMultiplier;
		if (flag4)
		{
			strainTimer += UnityEngine.Time.deltaTime * num4;
		}
		else
		{
			strainTimer = Mathf.MoveTowards(strainTimer, 0f, UnityEngine.Time.deltaTime * 1.5f);
		}
		float num5 = strainTimer / 6f;
		SetFlag(Flags.Reserved1, flag4 && num5 > 0.25f);
		if ((float)lastStrainUpdate > 0.4f || fishState != currentFishState)
		{
			ClientRPC(null, "Client_UpdateFishState", (int)currentFishState, num5);
			lastStrainUpdate = 0f;
		}
		if (strainTimer > 7f || ForceFail)
		{
			Server_Cancel(FailReason.TensionBreak);
		}
		else
		{
			if (!(num2 <= FishCatchDistance) && !ForceSuccess)
			{
				return;
			}
			CurrentState = CatchState.Caught;
			if (currentFishTarget != null)
			{
				Item item = ItemManager.Create(currentFishTarget, 1, 0uL);
				ownerPlayer.GiveItem(item, GiveItemReason.Crafted);
				if (currentFishTarget.shortname == "skull.human")
				{
					item.name = RandomUsernames.Get(UnityEngine.Random.Range(0, 1000));
				}
			}
			ClientRPC(null, "Client_OnCaughtFish", currentFishTarget.itemid);
			ownerPlayer.SignalBroadcast(Signal.Alt_Attack);
			Invoke(ResetLine, 6f);
			fishingBobber.Kill();
			currentBobber.Set(null);
			CancelInvoke(CatchProcess);
		}
	}

	private void ResetLine()
	{
		Server_Cancel(FailReason.Success);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Server_Cancel(RPCMessage msg)
	{
		if (CurrentState != CatchState.Caught)
		{
			Server_Cancel(FailReason.UserRequested);
		}
	}

	private void Server_Cancel(FailReason reason)
	{
		if (GetItem() != null)
		{
			GetItem().LoseCondition((reason == FailReason.Success) ? ConditionLossOnSuccess : ConditionLossOnFail);
		}
		SetFlag(Flags.Busy, false);
		UpdateFlags();
		CancelInvoke(CatchProcess);
		CurrentState = CatchState.None;
		SetFlag(Flags.Reserved1, false);
		FishingBobber fishingBobber = currentBobber.Get(true);
		if (fishingBobber != null)
		{
			fishingBobber.Kill();
			currentBobber.Set(null);
		}
		ClientRPC(null, "Client_ResetLine", (int)reason);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (CurrentState != 0)
		{
			Server_Cancel(FailReason.Unequipped);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (currentBobber.IsSet && info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Facepunch.Pool.Get<SimpleUID>();
			info.msg.simpleUID.uid = currentBobber.uid;
		}
	}

	private void UpdateFlags(bool inputLeft = false, bool inputRight = false, bool back = false)
	{
		SetFlag(ReelingInFlag, CurrentState == CatchState.Catching && back);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if ((!base.isServer || !info.fromDisk) && info.msg.simpleUID != null)
		{
			currentBobber.uid = info.msg.simpleUID.uid;
		}
	}

	public override bool BlocksGestures()
	{
		return CurrentState != CatchState.None;
	}

	private bool AllowPullInDirection(Vector3 worldDirection, Vector3 bobberPosition)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = bobberPosition.WithY(position.y);
		return Vector3.Dot(worldDirection, (vector - position).normalized) < 0f;
	}

	private bool EvaluateFishingPosition(ref Vector3 pos, BasePlayer ply, out FailReason reason, out WaterBody waterBody)
	{
		waterBody = null;
		bool flag = false;
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		GamePhysics.OverlapSphere(pos, 1f, obj, 16, QueryTriggerInteraction.Collide);
		if (obj.Count > 0)
		{
			foreach (Collider item in obj)
			{
				item.TryGetComponent<WaterBody>(out waterBody);
				if (waterBody == null)
				{
					waterBody = item.transform.parent.GetComponentInChildren<WaterBody>();
				}
				if (!(waterBody != null) || waterBody.FishingType == (WaterBody.FishingTag)0)
				{
					continue;
				}
				flag = true;
				RaycastHit hitInfo;
				if (GamePhysics.Trace(new Ray(pos + Vector3.up, -Vector3.up), 0f, out hitInfo, 1.5f, 16, QueryTriggerInteraction.Collide))
				{
					pos.y = hitInfo.point.y;
				}
				else
				{
					pos.y = obj[0].transform.position.y;
				}
				if (!waterBody.IsOcean)
				{
					if (waterBody.Renderer != null && (waterBody.FishingType & WaterBody.FishingTag.MoonPool) == WaterBody.FishingTag.MoonPool)
					{
						pos.y = waterBody.Renderer.transform.position.y;
					}
					break;
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		if (!flag)
		{
			reason = FailReason.NoWaterFound;
			return false;
		}
		if (Vector3.Distance(ply.transform.position.WithY(pos.y), pos) < 5f)
		{
			reason = FailReason.TooClose;
			return false;
		}
		if (!GamePhysics.LineOfSight(ply.eyes.position, pos, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 p = pos + Vector3.up * 2f;
		if (!GamePhysics.LineOfSight(ply.eyes.position, p, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 position = ply.transform.position;
		position.y = pos.y;
		float num = Vector3.Distance(pos, position);
		Vector3 p2 = pos + (position - pos).normalized * (num - FishCatchDistance);
		if (!GamePhysics.LineOfSight(pos, p2, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(Vector3.Lerp(pos, ply.transform.position.WithY(pos.y), 0.95f), true, null, true) < 0.1f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(pos, true, null, true) < 0.3f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		reason = FailReason.Success;
		return true;
	}

	private Item GetCurrentLure()
	{
		if (GetItem() == null)
		{
			return null;
		}
		if (GetItem().contents == null)
		{
			return null;
		}
		return GetItem().contents.GetSlot(0);
	}

	private bool HasReelInInput(InputState state)
	{
		if (!state.IsDown(BUTTON.BACKWARD))
		{
			return state.IsDown(BUTTON.FIRE_PRIMARY);
		}
		return true;
	}
}
