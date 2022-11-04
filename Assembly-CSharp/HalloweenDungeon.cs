using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class HalloweenDungeon : BasePortal
{
	public GameObjectRef dungeonPrefab;

	public EntityRef<ProceduralDynamicDungeon> dungeonInstance;

	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population;

	public float lifetime = 600f;

	private float secondsUsed;

	private float timeAlive;

	public AnimationCurve radiationCurve;

	public Translate.Phrase collapsePhrase;

	public Translate.Phrase mountPhrase;

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.ioEntity != null)
		{
			dungeonInstance.uid = info.msg.ioEntity.genericEntRef3;
			secondsUsed = info.msg.ioEntity.genericFloat1;
			timeAlive = info.msg.ioEntity.genericFloat2;
		}
	}

	public float GetLifeFraction()
	{
		return Mathf.Clamp01(secondsUsed / lifetime);
	}

	public void Update()
	{
		if (!base.isClient)
		{
			if (secondsUsed > 0f)
			{
				secondsUsed += Time.deltaTime;
			}
			timeAlive += Time.deltaTime;
			float lifeFraction = GetLifeFraction();
			if (dungeonInstance.IsValid(serverside: true))
			{
				ProceduralDynamicDungeon proceduralDynamicDungeon = dungeonInstance.Get(serverside: true);
				float value = radiationCurve.Evaluate(lifeFraction) * 100f;
				proceduralDynamicDungeon.exitRadiation.RadiationAmountOverride = Mathf.Clamp(value, 0f, float.PositiveInfinity);
			}
			if (lifeFraction >= 1f)
			{
				Kill();
			}
			else if (timeAlive > 3600f && secondsUsed == 0f)
			{
				Kill();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.ioEntity == null)
		{
			info.msg.ioEntity = Pool.Get<ProtoBuf.IOEntity>();
		}
		info.msg.ioEntity.genericEntRef3 = dungeonInstance.uid;
		info.msg.ioEntity.genericFloat1 = secondsUsed;
		info.msg.ioEntity.genericFloat2 = timeAlive;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		timeAlive += Random.Range(0f, 60f);
	}

	public override void UsePortal(BasePlayer player)
	{
		if (GetLifeFraction() > 0.8f)
		{
			player.ShowToast(GameTip.Styles.Blue_Normal, collapsePhrase);
			return;
		}
		if (player.isMounted)
		{
			player.ShowToast(GameTip.Styles.Blue_Normal, mountPhrase);
			return;
		}
		if (secondsUsed == 0f)
		{
			secondsUsed = 1f;
		}
		base.UsePortal(player);
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			timeAlive = Random.Range(0f, 60f);
			SpawnSubEntities();
		}
		TransformEx.DropToGround(localEntryExitPos, alignToNormal: false, 10f);
		localEntryExitPos.transform.position += Vector3.up * 0.05f;
		Invoke(CheckBlocked, 0.25f);
	}

	public void CheckBlocked()
	{
		float num = 0.5f;
		float num2 = 1.8f;
		Vector3 position = localEntryExitPos.position;
		Vector3 start = position + new Vector3(0f, num, 0f);
		Vector3 end = position + new Vector3(0f, num2 - num, 0f);
		if (Physics.CheckCapsule(start, end, num, 1537286401))
		{
			Kill();
		}
	}

	public static Vector3 GetDungeonSpawnPoint()
	{
		float num = 200f;
		float num2 = 200f;
		float num3 = Mathf.Floor(TerrainMeta.Size.x / 200f);
		float num4 = 1000f;
		Vector3 zero = Vector3.zero;
		zero.x = 0f - Mathf.Min(TerrainMeta.Size.x, 4000f) + num;
		zero.y = 1000f;
		zero.z = 0f - Mathf.Min(TerrainMeta.Size.z, 4000f) + num;
		_ = Vector3.zero;
		for (int i = 0; (float)i < num4; i++)
		{
			for (int j = 0; (float)j < num3; j++)
			{
				Vector3 vector = zero + new Vector3((float)j * num, (float)i * num2, 0f);
				bool flag = false;
				foreach (ProceduralDynamicDungeon dungeon in ProceduralDynamicDungeon.dungeons)
				{
					if (Vector3.Distance(dungeon.transform.position, vector) < 10f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return vector;
				}
			}
		}
		return Vector3.zero;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (dungeonInstance.IsValid(serverside: true))
		{
			dungeonInstance.Get(serverside: true).Kill();
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public void SpawnSubEntities()
	{
		Vector3 dungeonSpawnPoint = GetDungeonSpawnPoint();
		if (dungeonSpawnPoint == Vector3.zero)
		{
			Debug.LogError("No dungeon spawn point");
			Invoke(DelayedDestroy, 5f);
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(dungeonPrefab.resourcePath, dungeonSpawnPoint, Quaternion.identity);
		baseEntity.Spawn();
		ProceduralDynamicDungeon component = baseEntity.GetComponent<ProceduralDynamicDungeon>();
		dungeonInstance.Set(component);
		BasePortal basePortal = (targetPortal = component.GetExitPortal());
		basePortal.targetPortal = this;
		LinkPortal();
		basePortal.LinkPortal();
	}
}
