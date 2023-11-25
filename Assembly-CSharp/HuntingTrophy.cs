#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;

public class HuntingTrophy : StorageContainer
{
	[Serializable]
	public struct TrophyRoot
	{
		public GameObjectRef SourceEntity;

		public GameObject Root;

		public uint GetSourcePrefabId()
		{
			BaseEntity entity = SourceEntity.GetEntity();
			if (entity != null)
			{
				return entity.prefabID;
			}
			return 0u;
		}

		public bool Matches(HeadEntity headEnt)
		{
			BaseEntity entity = SourceEntity.GetEntity();
			bool flag = entity != null && headEnt.CurrentTrophyData != null && entity.prefabID == headEnt.CurrentTrophyData.entitySource;
			if (!flag)
			{
				GameObject headSource = headEnt.GetHeadSource();
				if (headSource != null && headSource.TryGetComponent<BasePlayer>(out var component) && entity.TryGetComponent<BasePlayer>(out component))
				{
					flag = true;
				}
			}
			return flag;
		}

		public bool Matches(HeadData data)
		{
			if (data == null)
			{
				return false;
			}
			BaseEntity entity = SourceEntity.GetEntity();
			bool flag = entity != null && entity.prefabID == data.entitySource;
			if (!flag)
			{
				GameObject gameObject = null;
				gameObject = GameManager.server.FindPrefab(data.entitySource);
				if (gameObject != null && gameObject.TryGetComponent<BasePlayer>(out var component) && entity.TryGetComponent<BasePlayer>(out component))
				{
					flag = true;
				}
			}
			return flag;
		}
	}

	private HeadData CurrentTrophyData;

	public PlayerModel Player;

	public GameObject MaleRope;

	public GameObject FemaleRope;

	public Renderer[] HorseRenderers;

	public Renderer[] HorseHairRenderers;

	public const uint HORSE_PREFAB_ID = 2421623959u;

	public GameObject NameRoot;

	public RustText NameText;

	public TrophyRoot[] Trophies;

	public HeadData TrophyData => CurrentTrophyData;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HuntingTrophy.OnRpcMessage"))
		{
			if (rpc == 1170506026 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerRequestClear ");
				}
				using (TimeWarning.New("ServerRequestClear"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1170506026u, "ServerRequestClear", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							ServerRequestClear();
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerRequestClear");
					}
				}
				return true;
			}
			if (rpc == 3878554182u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerRequestSubmit ");
				}
				using (TimeWarning.New("ServerRequestSubmit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3878554182u, "ServerRequestSubmit", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							ServerRequestSubmit();
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ServerRequestSubmit");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (ItemModAssociatedEntity<HeadEntity>.GetAssociatedEntity(item) == null)
		{
			return false;
		}
		return base.ItemFilter(item, targetSlot);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ServerRequestSubmit()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null)
		{
			return;
		}
		HeadEntity associatedEntity = ItemModAssociatedEntity<HeadEntity>.GetAssociatedEntity(slot);
		if (associatedEntity != null && !CanSubmitHead(associatedEntity))
		{
			return;
		}
		if (associatedEntity != null)
		{
			if (CurrentTrophyData == null)
			{
				CurrentTrophyData = Facepunch.Pool.Get<HeadData>();
				associatedEntity.CurrentTrophyData.CopyTo(CurrentTrophyData);
				CurrentTrophyData.count = 1u;
			}
			else
			{
				CurrentTrophyData.count++;
			}
		}
		slot.Remove();
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ServerRequestClear()
	{
		if (CurrentTrophyData != null)
		{
			Facepunch.Pool.Free(ref CurrentTrophyData);
			SendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (CurrentTrophyData != null)
		{
			info.msg.headData = Facepunch.Pool.Get<HeadData>();
			CurrentTrophyData.CopyTo(info.msg.headData);
		}
	}

	public bool CanSubmitHead(HeadEntity headEnt)
	{
		bool flag = false;
		bool flag2 = CurrentTrophyData != null;
		if (flag2 && headEnt.CurrentTrophyData.entitySource == CurrentTrophyData.entitySource && headEnt.CurrentTrophyData.playerId == CurrentTrophyData.playerId && headEnt.CurrentTrophyData.horseBreed == CurrentTrophyData.horseBreed)
		{
			flag = true;
		}
		if (!flag && flag2)
		{
			GameObject headSource = headEnt.GetHeadSource();
			if (headSource != null && headSource.TryGetComponent<BasePlayer>(out var _) && GetCurrentTrophyDataSource() == headSource)
			{
				flag = true;
			}
		}
		if (!flag2)
		{
			TrophyRoot[] trophies = Trophies;
			foreach (TrophyRoot trophyRoot in trophies)
			{
				if (trophyRoot.Matches(headEnt))
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
		GameObject GetCurrentTrophyDataSource()
		{
			return GameManager.server.FindPrefab(CurrentTrophyData.entitySource);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.headData != null)
		{
			if (CurrentTrophyData == null)
			{
				CurrentTrophyData = Facepunch.Pool.Get<HeadData>();
			}
			info.msg.headData.CopyTo(CurrentTrophyData);
		}
		else if (CurrentTrophyData != null)
		{
			Facepunch.Pool.Free(ref CurrentTrophyData);
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		if (CurrentTrophyData != null)
		{
			Facepunch.Pool.Free(ref CurrentTrophyData);
		}
		TrophyRoot[] trophies = Trophies;
		for (int i = 0; i < trophies.Length; i++)
		{
			TrophyRoot trophyRoot = trophies[i];
			if (trophyRoot.Root != null)
			{
				trophyRoot.Root.SetActive(value: false);
			}
		}
		if (NameRoot != null)
		{
			NameRoot.SetActive(value: false);
		}
		if (MaleRope != null)
		{
			MaleRope.SetActive(value: false);
		}
		if (FemaleRope != null)
		{
			FemaleRope.SetActive(value: false);
		}
	}
}
