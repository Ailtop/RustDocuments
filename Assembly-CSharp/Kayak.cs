using Network;
using Rust;
using UnityEngine;

public class Kayak : BaseBoat, IPoolVehicle
{
	private enum PaddleDirection
	{
		Left = 0,
		Right = 1,
		LeftBack = 2,
		RightBack = 3
	}

	public ItemDefinition OarItem;

	public float maxPaddleFrequency = 0.5f;

	public float forwardPaddleForce = 5f;

	public float multiDriverPaddleForceMultiplier = 0.75f;

	public float rotatePaddleForce = 3f;

	public GameObjectRef forwardSplashEffect;

	public GameObjectRef backSplashEffect;

	public ParticleSystem moveSplashEffect;

	public float animationLerpSpeed = 6f;

	[Header("Audio")]
	public BlendedSoundLoops waterLoops;

	public float waterSoundSpeedDivisor = 10f;

	public GameObjectRef pushLandEffect;

	public GameObjectRef pushWaterEffect;

	public PlayerModel.MountPoses noPaddlePose;

	public TimeSince[] playerPaddleCooldowns = new TimeSince[2];

	public TimeCachedValue<float> fixedDragUpdate;

	public TimeSince timeSinceLastUsed;

	private const float DECAY_TICK_TIME = 60f;

	private Vector3 lastTravelPos;

	private float distanceRemainder;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Kayak.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		timeSinceLastUsed = 0f;
		InvokeRandomized(BoatDecay, Random.Range(30f, 60f), 60f, 6f);
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		if (IsInvoking(TravelDistanceUpdate) || !Rust.GameInfo.HasAchievements)
		{
			return;
		}
		int num = 0;
		foreach (MountPointInfo allMountPoint in base.allMountPoints)
		{
			if (allMountPoint.mountable != null && allMountPoint.mountable.IsMounted())
			{
				num++;
			}
		}
		if (num == 2)
		{
			lastTravelPos = base.transform.position.WithY(0f);
			InvokeRandomized(TravelDistanceUpdate, 5f, 5f, 3f);
		}
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if (IsInvoking(TravelDistanceUpdate))
		{
			CancelInvoke(TravelDistanceUpdate);
		}
	}

	public override void DriverInput(InputState inputState, BasePlayer player)
	{
		timeSinceLastUsed = 0f;
		if (!IsPlayerHoldingPaddle(player))
		{
			return;
		}
		int playerSeat = GetPlayerSeat(player);
		if (!((float)playerPaddleCooldowns[playerSeat] > maxPaddleFrequency))
		{
			return;
		}
		bool flag = inputState.IsDown(BUTTON.BACKWARD);
		bool flag2 = false;
		Vector3 vector = base.transform.forward;
		if (flag)
		{
			vector = -vector;
		}
		float num = forwardPaddleForce;
		if (NumMounted() >= 2)
		{
			num *= multiDriverPaddleForceMultiplier;
		}
		if (inputState.IsDown(BUTTON.LEFT) || inputState.IsDown(BUTTON.FIRE_PRIMARY))
		{
			flag2 = true;
			rigidBody.AddForceAtPosition(vector * num, GetPaddlePoint(playerSeat, PaddleDirection.Left), ForceMode.Impulse);
			rigidBody.angularVelocity += -base.transform.up * rotatePaddleForce;
			ClientRPC(null, "OnPaddled", flag ? 2 : 0, playerSeat);
		}
		else if (inputState.IsDown(BUTTON.RIGHT) || inputState.IsDown(BUTTON.FIRE_SECONDARY))
		{
			flag2 = true;
			rigidBody.AddForceAtPosition(vector * num, GetPaddlePoint(playerSeat, PaddleDirection.Right), ForceMode.Impulse);
			rigidBody.angularVelocity += base.transform.up * rotatePaddleForce;
			ClientRPC(null, "OnPaddled", (!flag) ? 1 : 3, playerSeat);
		}
		if (flag2)
		{
			playerPaddleCooldowns[playerSeat] = 0f;
			if (!flag)
			{
				Vector3 velocity = rigidBody.velocity;
				rigidBody.velocity = Vector3.Lerp(velocity, vector * velocity.magnitude, 0.4f);
			}
		}
	}

	private void TravelDistanceUpdate()
	{
		Vector3 b = base.transform.position.WithY(0f);
		if (Rust.GameInfo.HasAchievements)
		{
			float num = Vector3.Distance(lastTravelPos, b) + distanceRemainder;
			float num2 = Mathf.Max(Mathf.Floor(num), 0f);
			distanceRemainder = num - num2;
			foreach (MountPointInfo allMountPoint in base.allMountPoints)
			{
				if (allMountPoint.mountable != null && allMountPoint.mountable.IsMounted() && (int)num2 > 0)
				{
					allMountPoint.mountable.GetMounted().stats.Add("kayak_distance_travelled", (int)num2);
				}
			}
		}
		lastTravelPos = b;
	}

	public override bool EngineOn()
	{
		return false;
	}

	public override void DoPushAction(BasePlayer player)
	{
		if (IsFlipped())
		{
			rigidBody.AddRelativeTorque(Vector3.forward * 8f, ForceMode.VelocityChange);
		}
		else
		{
			Vector3 vector = Vector3Ex.Direction2D(player.transform.position + player.eyes.BodyForward() * 3f, player.transform.position);
			vector = (Vector3.up * 0.1f + vector).normalized;
			Vector3 position = base.transform.position;
			float num = 5f;
			if (IsInWater())
			{
				num *= 0.75f;
			}
			rigidBody.AddForceAtPosition(vector * num, position, ForceMode.VelocityChange);
		}
		if (IsInWater())
		{
			if (pushWaterEffect.isValid)
			{
				Effect.server.Run(pushWaterEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
		else if (pushLandEffect.isValid)
		{
			Effect.server.Run(pushLandEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
		WakeUp();
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (fixedDragUpdate == null)
		{
			fixedDragUpdate = new TimeCachedValue<float>
			{
				refreshCooldown = 0.5f,
				refreshRandomRange = 0.2f,
				updateValue = CalculateDesiredDrag
			};
		}
		rigidBody.drag = fixedDragUpdate.Get(force: false);
	}

	public float CalculateDesiredDrag()
	{
		int num = NumMounted();
		if (num == 0)
		{
			return 0.5f;
		}
		if (num < 2)
		{
			return 0.05f;
		}
		return 0.1f;
	}

	public void BoatDecay()
	{
		BaseBoat.WaterVehicleDecay(this, 60f, timeSinceLastUsed, MotorRowboat.outsidedecayminutes, MotorRowboat.deepwaterdecayminutes);
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (!HasDriver())
		{
			return base.CanPickup(player);
		}
		return false;
	}

	public bool IsPlayerHoldingPaddle(BasePlayer player)
	{
		if (player.GetHeldEntity() != null)
		{
			return player.GetHeldEntity().GetItem().info == OarItem;
		}
		return false;
	}

	public Vector3 GetPaddlePoint(int index, PaddleDirection direction)
	{
		index = Mathf.Clamp(index, 0, mountPoints.Count);
		Vector3 pos = mountPoints[index].pos;
		switch (direction)
		{
		case PaddleDirection.Left:
			pos.x -= 1f;
			break;
		case PaddleDirection.Right:
			pos.x += 1f;
			break;
		}
		pos.y -= 0.2f;
		return base.transform.TransformPoint(pos);
	}

	public bool IsInWater()
	{
		if (base.isServer)
		{
			return buoyancy.timeOutOfWater < 0.1f;
		}
		return false;
	}
}
