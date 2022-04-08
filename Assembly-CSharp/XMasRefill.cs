using System.Collections.Generic;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;

public class XMasRefill : BaseEntity
{
	public GameObjectRef[] giftPrefabs;

	public List<BasePlayer> goodKids;

	public List<Stocking> stockings;

	public AudioSource bells;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("XMasRefill.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public float GiftRadius()
	{
		return XMas.spawnRange;
	}

	public int GiftsPerPlayer()
	{
		return XMas.giftsPerPlayer;
	}

	public int GiftSpawnAttempts()
	{
		return XMas.giftsPerPlayer * XMas.spawnAttempts;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!XMas.enabled)
		{
			Invoke(RemoveMe, 0.1f);
			return;
		}
		goodKids = ((BasePlayer.activePlayerList != null) ? new List<BasePlayer>(BasePlayer.activePlayerList) : new List<BasePlayer>());
		stockings = ((Stocking.stockings != null) ? new List<Stocking>(Stocking.stockings.Values) : new List<Stocking>());
		Invoke(RemoveMe, 60f);
		if (Interface.CallHook("OnXmasLootDistribute", this) == null)
		{
			InvokeRepeating(DistributeLoot, 3f, 0.02f);
			Invoke(SendBells, 0.5f);
		}
	}

	public void SendBells()
	{
		ClientRPC(null, "PlayBells");
	}

	public void RemoveMe()
	{
		if (goodKids.Count == 0 && stockings.Count == 0)
		{
			Kill();
		}
		else
		{
			Invoke(RemoveMe, 60f);
		}
	}

	public void DistributeLoot()
	{
		if (goodKids.Count > 0)
		{
			BasePlayer basePlayer = null;
			foreach (BasePlayer goodKid in goodKids)
			{
				if (!goodKid.IsSleeping() && !goodKid.IsWounded() && goodKid.IsAlive())
				{
					basePlayer = goodKid;
					break;
				}
			}
			if ((bool)basePlayer)
			{
				DistributeGiftsForPlayer(basePlayer);
				goodKids.Remove(basePlayer);
			}
		}
		if (stockings.Count > 0)
		{
			Stocking stocking = stockings[0];
			if (stocking != null)
			{
				stocking.SpawnLoot();
			}
			stockings.RemoveAt(0);
		}
	}

	protected bool DropToGround(ref Vector3 pos)
	{
		int num = 1235288065;
		int num2 = 8454144;
		if ((bool)TerrainMeta.TopologyMap && ((uint)TerrainMeta.TopologyMap.GetTopology(pos) & 0x14080u) != 0)
		{
			return false;
		}
		if ((bool)TerrainMeta.HeightMap && (bool)TerrainMeta.Collision && !TerrainMeta.Collision.GetIgnore(pos))
		{
			float height = TerrainMeta.HeightMap.GetHeight(pos);
			pos.y = Mathf.Max(pos.y, height);
		}
		if (!TransformUtil.GetGroundInfo(pos, out var hitOut, 80f, num))
		{
			return false;
		}
		if (((1 << hitOut.transform.gameObject.layer) & num2) == 0)
		{
			return false;
		}
		pos = hitOut.point;
		return true;
	}

	public bool DistributeGiftsForPlayer(BasePlayer player)
	{
		if (Interface.CallHook("OnXmasGiftsDistribute", this, player) != null)
		{
			return false;
		}
		int num = GiftsPerPlayer();
		int num2 = GiftSpawnAttempts();
		for (int i = 0; i < num2; i++)
		{
			if (num <= 0)
			{
				break;
			}
			Vector2 vector = UnityEngine.Random.insideUnitCircle * GiftRadius();
			Vector3 pos = player.transform.position + new Vector3(vector.x, 10f, vector.y);
			Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			if (DropToGround(ref pos))
			{
				string resourcePath = giftPrefabs[UnityEngine.Random.Range(0, giftPrefabs.Length)].resourcePath;
				BaseEntity baseEntity = GameManager.server.CreateEntity(resourcePath, pos, rot);
				if ((bool)baseEntity)
				{
					baseEntity.Spawn();
					num--;
				}
			}
		}
		return true;
	}
}
