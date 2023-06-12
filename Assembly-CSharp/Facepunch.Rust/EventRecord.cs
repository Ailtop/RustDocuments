using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Facepunch.Rust;

public class EventRecord : Pool.IPooled
{
	public DateTime Timestamp;

	[NonSerialized]
	public bool IsServer;

	public List<EventRecordField> Data = new List<EventRecordField>();

	public string EventType { get; private set; }

	public void EnterPool()
	{
		Timestamp = default(DateTime);
		EventType = null;
		IsServer = false;
		Data.Clear();
	}

	public void LeavePool()
	{
	}

	public static EventRecord New(string type, bool isServer = true)
	{
		EventRecord eventRecord = Pool.Get<EventRecord>();
		eventRecord.EventType = type;
		eventRecord.AddField("type", type);
		eventRecord.AddField("guid", Guid.NewGuid());
		eventRecord.IsServer = isServer;
		if (isServer)
		{
			eventRecord.AddField("wipe_id", SaveRestore.WipeId);
		}
		eventRecord.Timestamp = DateTime.UtcNow;
		return eventRecord;
	}

	public EventRecord AddObject(string key, object data)
	{
		Data.Add(new EventRecordField(key)
		{
			String = JsonConvert.SerializeObject(data),
			IsObject = true
		});
		return this;
	}

	public EventRecord SetTimestamp(DateTime timestamp)
	{
		Timestamp = timestamp;
		return this;
	}

	public EventRecord AddField(string key, bool value)
	{
		Data.Add(new EventRecordField(key)
		{
			String = (value ? "true" : "false")
		});
		return this;
	}

	public EventRecord AddField(string key, string value)
	{
		Data.Add(new EventRecordField(key)
		{
			String = value
		});
		return this;
	}

	public EventRecord AddField(string key, int value)
	{
		Data.Add(new EventRecordField(key)
		{
			Number = value
		});
		return this;
	}

	public EventRecord AddField(string key, uint value)
	{
		Data.Add(new EventRecordField(key)
		{
			Number = value
		});
		return this;
	}

	public EventRecord AddField(string key, ulong value)
	{
		Data.Add(new EventRecordField(key)
		{
			Number = (long)value
		});
		return this;
	}

	public EventRecord AddField(string key, long value)
	{
		Data.Add(new EventRecordField(key)
		{
			Number = value
		});
		return this;
	}

	public EventRecord AddField(string key, float value)
	{
		Data.Add(new EventRecordField(key)
		{
			Float = value
		});
		return this;
	}

	public EventRecord AddField(string key, double value)
	{
		Data.Add(new EventRecordField(key)
		{
			Float = value
		});
		return this;
	}

	public EventRecord AddField(string key, TimeSpan value)
	{
		Data.Add(new EventRecordField(key)
		{
			Float = value.TotalSeconds
		});
		return this;
	}

	public EventRecord AddField(string key, Guid value)
	{
		Data.Add(new EventRecordField(key)
		{
			Guid = value
		});
		return this;
	}

	public EventRecord AddField(string key, Vector3 value)
	{
		Data.Add(new EventRecordField(key)
		{
			Vector = value
		});
		return this;
	}

	public EventRecord AddField(string key, BaseEntity entity)
	{
		if (entity?.net == null)
		{
			return this;
		}
		if (entity is BasePlayer basePlayer && !basePlayer.IsNpc && !basePlayer.IsBot)
		{
			string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(basePlayer.userID);
			Data.Add(new EventRecordField(key, "_userid")
			{
				String = userWipeId
			});
			if (basePlayer.isMounted)
			{
				AddField(key + "_mounted", basePlayer.GetMounted());
			}
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				Data.Add(new EventRecordField(key, "_admin")
				{
					String = "true"
				});
			}
		}
		if (entity is BaseProjectile baseProjectile)
		{
			Item item = baseProjectile.GetItem();
			if (item != null && (item.contents?.itemList?.Count ?? 0) > 0)
			{
				List<string> obj = Pool.GetList<string>();
				foreach (Item item2 in item.contents.itemList)
				{
					obj.Add(item2.info.shortname);
				}
				AddObject(key + "_inventory", obj);
				Pool.FreeList(ref obj);
			}
		}
		if (entity is BuildingBlock buildingBlock)
		{
			Data.Add(new EventRecordField(key, "_grade")
			{
				Number = (long)buildingBlock.grade
			});
		}
		Data.Add(new EventRecordField(key, "_prefab")
		{
			String = entity.ShortPrefabName
		});
		Data.Add(new EventRecordField(key, "_pos")
		{
			Vector = entity.transform.position
		});
		Data.Add(new EventRecordField(key, "_rot")
		{
			Vector = entity.transform.rotation.eulerAngles
		});
		Data.Add(new EventRecordField(key, "_id")
		{
			Number = (long)entity.net.ID.Value
		});
		return this;
	}

	public EventRecord AddField(string key, Item item)
	{
		Data.Add(new EventRecordField(key, "_name")
		{
			String = item.info.shortname
		});
		Data.Add(new EventRecordField(key, "_amount")
		{
			Number = item.amount
		});
		Data.Add(new EventRecordField(key, "_skin")
		{
			Number = (long)item.skin
		});
		Data.Add(new EventRecordField(key, "_condition")
		{
			Float = item.conditionNormalized
		});
		return this;
	}

	public void Submit()
	{
		if (IsServer)
		{
			if (Analytics.StatsBlacklist != null && Analytics.StatsBlacklist.Contains(EventType))
			{
				EventRecord obj = this;
				Pool.Free(ref obj);
			}
			else
			{
				Analytics.AzureWebInterface.server.EnqueueEvent(this);
			}
		}
	}
}
