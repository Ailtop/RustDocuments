#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class NeonSign : Signage
{
	private const float FastSpeed = 0.5f;

	private const float MediumSpeed = 1f;

	private const float SlowSpeed = 2f;

	private const float MinSpeed = 0.5f;

	private const float MaxSpeed = 5f;

	[Header("Neon Sign")]
	public Light topLeft;

	public Light topRight;

	public Light bottomLeft;

	public Light bottomRight;

	public float lightIntensity = 2f;

	[Range(1f, 100f)]
	public int powerConsumption = 10;

	public Material activeMaterial;

	public Material inactiveMaterial;

	public float animationSpeed = 1f;

	public int currentFrame;

	public List<ProtoBuf.NeonSign.Lights> frameLighting;

	public bool isAnimating;

	public Action animationLoopAction;

	public AmbienceEmitter ambientSoundEmitter;

	public SoundDefinition switchSoundDef;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NeonSign.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 2433901419u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetAnimationSpeed "));
				}
				using (TimeWarning.New("SetAnimationSpeed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2433901419u, "SetAnimationSpeed", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2433901419u, "SetAnimationSpeed", this, player, 3f))
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
							RPCMessage rPCMessage2 = rPCMessage;
							SetAnimationSpeed(rPCMessage2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetAnimationSpeed");
					}
				}
				return true;
			}
			if (rpc == 1919786296 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - UpdateNeonColors "));
				}
				using (TimeWarning.New("UpdateNeonColors"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1919786296u, "UpdateNeonColors", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1919786296u, "UpdateNeonColors", this, player, 3f))
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
							UpdateNeonColors(msg2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in UpdateNeonColors");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.neonSign == null)
		{
			return;
		}
		if (frameLighting != null)
		{
			foreach (ProtoBuf.NeonSign.Lights item in frameLighting)
			{
				ProtoBuf.NeonSign.Lights obj = item;
				Facepunch.Pool.Free(ref obj);
			}
			Facepunch.Pool.FreeList(ref frameLighting);
		}
		frameLighting = info.msg.neonSign.frameLighting;
		info.msg.neonSign.frameLighting = null;
		currentFrame = Mathf.Clamp(info.msg.neonSign.currentFrame, 0, paintableSources.Length);
		animationSpeed = Mathf.Clamp(info.msg.neonSign.animationSpeed, 0.5f, 5f);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		animationLoopAction = SwitchToNextFrame;
	}

	public override void ResetState()
	{
		base.ResetState();
		CancelInvoke(animationLoopAction);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (paintableSources.Length <= 1)
		{
			return;
		}
		bool flag = HasFlag(Flags.Reserved8);
		if (flag && !isAnimating)
		{
			if (currentFrame != 0)
			{
				currentFrame = 0;
				ClientRPC(null, "SetFrame", currentFrame);
			}
			InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
			isAnimating = true;
		}
		else if (!flag && isAnimating)
		{
			CancelInvoke(animationLoopAction);
			isAnimating = false;
		}
	}

	private void SwitchToNextFrame()
	{
		int num = currentFrame;
		for (int i = 0; i < paintableSources.Length; i++)
		{
			currentFrame++;
			if (currentFrame >= paintableSources.Length)
			{
				currentFrame = 0;
			}
			if (textureIDs[currentFrame] != 0)
			{
				break;
			}
		}
		if (currentFrame != num)
		{
			ClientRPC(null, "SetFrame", currentFrame);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		List<ProtoBuf.NeonSign.Lights> list = Facepunch.Pool.GetList<ProtoBuf.NeonSign.Lights>();
		if (frameLighting != null)
		{
			foreach (ProtoBuf.NeonSign.Lights item in frameLighting)
			{
				list.Add(item.Copy());
			}
		}
		info.msg.neonSign = Facepunch.Pool.Get<ProtoBuf.NeonSign>();
		info.msg.neonSign.frameLighting = list;
		info.msg.neonSign.currentFrame = currentFrame;
		info.msg.neonSign.animationSpeed = animationSpeed;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public void SetAnimationSpeed(RPCMessage msg)
	{
		float num = (animationSpeed = Mathf.Clamp(msg.read.Float(), 0.5f, 5f));
		if (isAnimating)
		{
			CancelInvoke(animationLoopAction);
			InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public void UpdateNeonColors(RPCMessage msg)
	{
		if (CanUpdateSign(msg.player))
		{
			int num = msg.read.Int32();
			if (num >= 0 && num < paintableSources.Length)
			{
				EnsureInitialized();
				frameLighting[num].topLeft = ClampColor(msg.read.Color());
				frameLighting[num].topRight = ClampColor(msg.read.Color());
				frameLighting[num].bottomLeft = ClampColor(msg.read.Color());
				frameLighting[num].bottomRight = ClampColor(msg.read.Color());
				SendNetworkUpdate();
			}
		}
	}

	public new void EnsureInitialized()
	{
		if (frameLighting == null)
		{
			frameLighting = Facepunch.Pool.GetList<ProtoBuf.NeonSign.Lights>();
		}
		while (frameLighting.Count < paintableSources.Length)
		{
			ProtoBuf.NeonSign.Lights lights = Facepunch.Pool.Get<ProtoBuf.NeonSign.Lights>();
			lights.topLeft = Color.clear;
			lights.topRight = Color.clear;
			lights.bottomLeft = Color.clear;
			lights.bottomRight = Color.clear;
			frameLighting.Add(lights);
		}
	}

	private static Color ClampColor(Color color)
	{
		return new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), Mathf.Clamp01(color.a));
	}
}
