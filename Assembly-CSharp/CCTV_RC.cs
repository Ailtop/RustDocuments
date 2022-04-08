#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class CCTV_RC : PoweredRemoteControlEntity
{
	public Transform pivotOrigin;

	public Transform yaw;

	public Transform pitch;

	public Vector2 pitchClamp = new Vector2(-50f, 50f);

	public Vector2 yawClamp = new Vector2(-50f, 50f);

	public float turnSpeed = 25f;

	public float serverLerpSpeed = 15f;

	public float clientLerpSpeed = 10f;

	public float pitchAmount;

	public float yawAmount;

	public bool hasPTZ = true;

	public const Flags Flag_HasViewer = Flags.Reserved5;

	public int numViewers;

	private bool externalViewer;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CCTV_RC.OnRpcMessage"))
		{
			if (rpc == 3353964129u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_SetDir "));
				}
				using (TimeWarning.New("Server_SetDir"))
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
							Server_SetDir(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_SetDir");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!base.isClient && IsStatic())
		{
			pitchAmount = pitch.localEulerAngles.x;
			yawAmount = yaw.localEulerAngles.y;
			UpdateRCAccess(isOnline: true);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateRotation(10000f);
	}

	public override void UserInput(InputState inputState, BasePlayer player)
	{
		if (hasPTZ)
		{
			float num = 1f;
			float num2 = Mathf.Clamp(0f - inputState.current.mouseDelta.y, -1f, 1f);
			float num3 = Mathf.Clamp(inputState.current.mouseDelta.x, -1f, 1f);
			pitchAmount = Mathf.Clamp(pitchAmount + num2 * num * turnSpeed, pitchClamp.x, pitchClamp.y);
			yawAmount = Mathf.Clamp(yawAmount + num3 * num * turnSpeed, yawClamp.x, yawClamp.y);
			Quaternion localRotation = Quaternion.Euler(pitchAmount, 0f, 0f);
			Quaternion localRotation2 = Quaternion.Euler(0f, yawAmount, 0f);
			pitch.transform.localRotation = localRotation;
			yaw.transform.localRotation = localRotation2;
			if (num2 != 0f || num3 != 0f)
			{
				SendNetworkUpdate();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.rcEntity.aim.x = pitchAmount;
		info.msg.rcEntity.aim.y = yawAmount;
		info.msg.rcEntity.aim.z = 0f;
	}

	[RPC_Server]
	public void Server_SetDir(RPCMessage msg)
	{
		if (!IsStatic())
		{
			BasePlayer player = msg.player;
			if (player.CanBuild() && player.IsBuildingAuthed() && Interface.CallHook("OnCCTVDirectionChange", this, player) == null)
			{
				Vector3 direction = Vector3Ex.Direction(player.eyes.position, yaw.transform.position);
				direction = base.transform.InverseTransformDirection(direction);
				Vector3 vector = BaseMountable.ConvertVector(Quaternion.LookRotation(direction).eulerAngles);
				pitchAmount = vector.x;
				yawAmount = vector.y;
				pitchAmount = Mathf.Clamp(pitchAmount, pitchClamp.x, pitchClamp.y);
				yawAmount = Mathf.Clamp(yawAmount, yawClamp.x, yawClamp.y);
				Quaternion localRotation = Quaternion.Euler(pitchAmount, 0f, 0f);
				Quaternion localRotation2 = Quaternion.Euler(0f, yawAmount, 0f);
				pitch.transform.localRotation = localRotation;
				yaw.transform.localRotation = localRotation2;
				SendNetworkUpdate();
			}
		}
	}

	public override void InitializeControl(BasePlayer controller)
	{
		base.InitializeControl(controller);
		numViewers++;
		UpdateViewers();
	}

	public override void StopControl()
	{
		base.StopControl();
		numViewers--;
		UpdateViewers();
	}

	public void PingFromExternalViewer()
	{
		Invoke(ResetExternalViewer, 10f);
		externalViewer = true;
		UpdateViewers();
	}

	private void ResetExternalViewer()
	{
		externalViewer = false;
		UpdateViewers();
	}

	public void UpdateViewers()
	{
		SetFlag(Flags.Reserved5, externalViewer || numViewers > 0);
	}

	public void UpdateRotation(float delta)
	{
		Quaternion b = Quaternion.Euler(pitchAmount, 0f, 0f);
		Quaternion b2 = Quaternion.Euler(0f, yawAmount, 0f);
		float t = delta * (base.isServer ? serverLerpSpeed : clientLerpSpeed);
		pitch.transform.localRotation = Quaternion.Lerp(pitch.transform.localRotation, b, t);
		yaw.transform.localRotation = Quaternion.Lerp(yaw.transform.localRotation, b2, t);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null)
		{
			pitchAmount = info.msg.rcEntity.aim.x;
			yawAmount = info.msg.rcEntity.aim.y;
		}
	}
}
