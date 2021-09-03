#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class InstantCameraTool : HeldEntity
{
	public ItemDefinition photoItem;

	public GameObjectRef screenshotEffect;

	public SoundDefinition startPhotoSoundDef;

	public SoundDefinition finishPhotoSoundDef;

	[Range(640f, 1920f)]
	public int resolutionX = 640;

	[Range(480f, 1080f)]
	public int resolutionY = 480;

	[Range(10f, 100f)]
	public int quality = 75;

	[Range(0f, 5f)]
	public float cooldownSeconds = 3f;

	private TimeSince _sinceLastPhoto;

	private bool hasSentAchievement;

	public const string PhotographPlayerAchievement = "SUMMER_PAPARAZZI";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("InstantCameraTool.OnRpcMessage"))
		{
			if (rpc == 3122234259u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - TakePhoto "));
				}
				using (TimeWarning.New("TakePhoto"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3122234259u, "TakePhoto", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3122234259u, "TakePhoto", this, player))
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
							RPCMessage msg2 = rPCMessage;
							TakePhoto(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in TakePhoto");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	private void TakePhoto(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		Item item = GetItem();
		if (player == null || item == null || item.condition <= 0f)
		{
			return;
		}
		byte[] array = msg.read.BytesWithSize();
		if (array.Length > 102400 || !ImageProcessing.IsValidJPG(array, resolutionX, resolutionY))
		{
			return;
		}
		Item item2 = ItemManager.Create(photoItem, 1, 0uL);
		if (item2 == null)
		{
			Debug.LogError("Failed to create photo item");
			return;
		}
		if (item2.instanceData.subEntity == 0)
		{
			item2.Remove();
			Debug.LogError("Photo has no sub-entity");
			return;
		}
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(item2.instanceData.subEntity);
		if (baseNetworkable == null)
		{
			item2.Remove();
			Debug.LogError("Sub-entity was not found");
			return;
		}
		PhotoEntity photoEntity;
		if ((object)(photoEntity = baseNetworkable as PhotoEntity) == null)
		{
			item2.Remove();
			Debug.LogError("Sub-entity is not a photo");
			return;
		}
		photoEntity.SetImageData(player.userID, array);
		if (!player.inventory.GiveItem(item2))
		{
			item2.Drop(player.GetDropPosition(), player.GetDropVelocity());
		}
		EffectNetwork.Send(new Effect(screenshotEffect.resourcePath, base.transform.position, base.transform.forward, msg.connection));
		if (!hasSentAchievement && !string.IsNullOrEmpty("SUMMER_PAPARAZZI"))
		{
			Vector3 position = GetOwnerPlayer().eyes.position;
			Vector3 vector = GetOwnerPlayer().eyes.HeadForward();
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			Vis.Entities(position + vector * 5f, 5f, obj, 131072);
			foreach (BasePlayer item3 in obj)
			{
				if (item3.isServer && item3 != GetOwnerPlayer() && item3.IsVisible(GetOwnerPlayer().eyes.position))
				{
					hasSentAchievement = true;
					GetOwnerPlayer().GiveAchievement("SUMMER_PAPARAZZI");
					break;
				}
			}
			Facepunch.Pool.FreeList(ref obj);
		}
		item.LoseCondition(1f);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy)
	{
		base.OnDeployed(parent, deployedBy);
		hasSentAchievement = false;
	}
}
