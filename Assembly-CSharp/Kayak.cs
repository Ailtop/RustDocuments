using Network;
using Rust;
using UnityEngine;

public class Kayak : BaseBoat, PoolVehicle
{
	private enum PaddleDirection
	{
		Left,
		Right,
		LeftBack,
		RightBack
	}

	public ItemDefinition OarItem;

	public float maxPaddleFrequency = 0.5f;

	public float forwardPaddleForce = 5f;

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

	public TimeSince lastUsedTime;

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
		InvokeRandomized(BoatDecay, Random.Range(30f, 60f), 60f, 6f);
	}

	public override void DriverInput(InputState inputState, BasePlayer player)
	{
		lastUsedTime = 0f;
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
		Vector3 a = base.transform.forward;
		if (flag)
		{
			a = -a;
		}
		if (inputState.IsDown(BUTTON.LEFT) || inputState.IsDown(BUTTON.FIRE_PRIMARY))
		{
			flag2 = true;
			rigidBody.AddForceAtPosition(a * forwardPaddleForce, GetPaddlePoint(playerSeat, PaddleDirection.Left), ForceMode.Impulse);
			rigidBody.angularVelocity += -base.transform.up * rotatePaddleForce;
			ClientRPC(null, "OnPaddled", flag ? 2 : 0, playerSeat);
		}
		else if (inputState.IsDown(BUTTON.RIGHT) || inputState.IsDown(BUTTON.FIRE_SECONDARY))
		{
			flag2 = true;
			rigidBody.AddForceAtPosition(a * forwardPaddleForce, GetPaddlePoint(playerSeat, PaddleDirection.Right), ForceMode.Impulse);
			rigidBody.angularVelocity += base.transform.up * rotatePaddleForce;
			ClientRPC(null, "OnPaddled", (!flag) ? 1 : 3, playerSeat);
		}
		if (flag2)
		{
			playerPaddleCooldowns[playerSeat] = 0f;
			if (!flag)
			{
				Vector3 velocity = rigidBody.velocity;
				rigidBody.velocity = Vector3.Lerp(velocity, a * velocity.magnitude, 0.4f);
			}
		}
	}

	public override bool EngineOn()
	{
		return false;
	}

	public override void DoPushAction(BasePlayer player)
	{
		if (HasDriver())
		{
			return;
		}
		player.metabolism.calories.Subtract(2f);
		player.metabolism.SendChangesToClient();
		if (IsFlipped())
		{
			rigidBody.AddRelativeTorque(Vector3.forward * 5f, ForceMode.VelocityChange);
		}
		else
		{
			Vector3 b = Vector3Ex.Direction2D(player.transform.position + player.eyes.BodyForward() * 3f, player.transform.position);
			b = (Vector3.up * 0.1f + b).normalized;
			Vector3 position = base.transform.position;
			float num = 5f;
			if (IsInWater())
			{
				num *= 0.75f;
			}
			rigidBody.AddForceAtPosition(b * num, position, ForceMode.VelocityChange);
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
	}

	protected override void VehicleFixedUpdate()
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
		rigidBody.drag = fixedDragUpdate.Get(false);
	}

	public float CalculateDesiredDrag()
	{
		if (NumMounted() != 0)
		{
			return 0.05f;
		}
		return 0.5f;
	}

	public void BoatDecay()
	{
		if (base.healthFraction != 0f && !((float)lastUsedTime > 600f))
		{
			float num = 1f / MotorRowboat.outsidedecayminutes;
			if (IsOutside())
			{
				Hurt(MaxHealth() * num, DamageType.Decay, this, false);
			}
		}
	}

	public void OnPoolDestroyed()
	{
		Kill(DestroyMode.Gib);
	}

	public void WakeUp()
	{
		if (rigidBody != null)
		{
			rigidBody.WakeUp();
			rigidBody.AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
		}
		if (buoyancy != null)
		{
			buoyancy.Wake();
		}
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
