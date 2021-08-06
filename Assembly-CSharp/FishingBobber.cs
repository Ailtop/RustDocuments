using UnityEngine;

public class FishingBobber : BaseCombatEntity
{
	public Transform centerOfMass;

	public Rigidbody myRigidBody;

	public Transform lineAttachPoint;

	public Transform bobberRoot;

	public const Flags CaughtFish = Flags.Reserved1;

	public float HorizontalMoveSpeed = 1f;

	public float PullAwayMoveSpeed = 1f;

	public float SidewaysInputForce = 1f;

	public float ReelInMoveSpeed = 1f;

	private float bobberForcePingPong;

	private Vector3 initialDirection;

	private Vector3 initialTargetPosition;

	private Vector3 spawnPosition;

	private TimeSince initialCastTime;

	private float initialDistance;

	public float TireAmount { get; private set; }

	public override void ServerInit()
	{
		myRigidBody.centerOfMass = centerOfMass.localPosition;
		base.ServerInit();
	}

	public void InitialiseBobber(BasePlayer forPlayer, WaterBody forBody, Vector3 targetPos)
	{
		initialDirection = forPlayer.eyes.HeadForward().WithY(0f);
		spawnPosition = base.transform.position;
		initialTargetPosition = targetPos;
		initialCastTime = 0f;
		initialDistance = Vector3.Distance(targetPos, forPlayer.transform.position.WithY(targetPos.y));
		InvokeRepeating(ProcessInitialCast, 0f, 0f);
	}

	private void ProcessInitialCast()
	{
		float num = 0.8f;
		if ((float)initialCastTime > num)
		{
			base.transform.position = initialTargetPosition;
			CancelInvoke(ProcessInitialCast);
			return;
		}
		float t = (float)initialCastTime / num;
		Vector3 vector = Vector3.Lerp(spawnPosition, initialTargetPosition, 0.5f);
		vector.y += 1.5f;
		Vector3 position = Vector3.Lerp(Vector3.Lerp(spawnPosition, vector, t), Vector3.Lerp(vector, initialTargetPosition, t), t);
		base.transform.position = position;
	}

	public void ServerMovementUpdate(bool inputLeft, bool inputRight, bool inputBack, ref BaseFishingRod.FishState state, Vector3 playerPos, ItemModFishable fishableModifier)
	{
		Vector3 normalized = (playerPos - base.transform.position).normalized;
		Vector3 vector = Vector3.zero;
		bobberForcePingPong = Mathf.Clamp(Mathf.PingPong(Time.time, 2f), 0.2f, 2f);
		if (FishStateExtensions.Contains(state, BaseFishingRod.FishState.PullingLeft))
		{
			vector = base.transform.right * (Time.deltaTime * HorizontalMoveSpeed * bobberForcePingPong * fishableModifier.MoveMultiplier * (inputRight ? 0.5f : 1f));
		}
		if (FishStateExtensions.Contains(state, BaseFishingRod.FishState.PullingRight))
		{
			vector = -base.transform.right * (Time.deltaTime * HorizontalMoveSpeed * bobberForcePingPong * fishableModifier.MoveMultiplier * (inputLeft ? 0.5f : 1f));
		}
		if (FishStateExtensions.Contains(state, BaseFishingRod.FishState.PullingBack))
		{
			vector += -base.transform.forward * (Time.deltaTime * PullAwayMoveSpeed * bobberForcePingPong * fishableModifier.MoveMultiplier * (inputBack ? 0.5f : 1f));
		}
		if (inputLeft || inputRight)
		{
			float num = 0.8f;
			if ((inputLeft && state == BaseFishingRod.FishState.PullingRight) || (inputRight && state == BaseFishingRod.FishState.PullingLeft))
			{
				num = 1.25f;
			}
			TireAmount += Time.deltaTime * num;
		}
		else
		{
			TireAmount -= Time.deltaTime * 0.1f;
		}
		if (inputLeft && !FishStateExtensions.Contains(state, BaseFishingRod.FishState.PullingLeft))
		{
			vector += base.transform.right * (Time.deltaTime * SidewaysInputForce);
		}
		else if (inputRight && !FishStateExtensions.Contains(state, BaseFishingRod.FishState.PullingRight))
		{
			vector += -base.transform.right * (Time.deltaTime * SidewaysInputForce);
		}
		if (inputBack)
		{
			float num2 = Mathx.RemapValClamped(TireAmount, 0f, 5f, 1f, 3f);
			vector += normalized * (ReelInMoveSpeed * fishableModifier.ReelInSpeedMultiplier * num2 * Time.deltaTime);
		}
		base.transform.LookAt(playerPos.WithY(base.transform.position.y));
		Vector3 vector2 = base.transform.position + vector;
		if (!IsDirectionValid(vector2, vector.magnitude, playerPos))
		{
			state = FishStateExtensions.FlipHorizontal(state);
		}
		else
		{
			base.transform.position = vector2;
		}
	}

	public bool IsDirectionValid(BaseFishingRod.FishState direction, float checkLength, Vector3 playerPos)
	{
		Vector3 vector = Vector3.zero;
		if (FishStateExtensions.Contains(direction, BaseFishingRod.FishState.PullingLeft))
		{
			vector = base.transform.right;
		}
		if (FishStateExtensions.Contains(direction, BaseFishingRod.FishState.PullingRight))
		{
			vector = -base.transform.right;
		}
		return IsDirectionValid(base.transform.position + vector * checkLength, checkLength, playerPos);
	}

	private bool IsDirectionValid(Vector3 pos, float checkLength, Vector3 playerPos)
	{
		if (Vector3.Angle((pos - playerPos).normalized.WithY(0f), initialDirection) > 60f)
		{
			return false;
		}
		Vector3 position = base.transform.position;
		RaycastHit hitInfo;
		if (GamePhysics.Trace(new Ray(position, (pos - position).normalized), 0.1f, out hitInfo, checkLength, 1218511105))
		{
			return false;
		}
		return true;
	}
}
