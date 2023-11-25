using UnityEngine;

namespace ConVar;

[Factory("heli")]
public class PatrolHelicopter : ConsoleSystem
{
	private const string path = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";

	[ServerVar]
	public static float lifetimeMinutes = 15f;

	[ServerVar]
	public static int guns = 1;

	[ServerVar]
	public static float bulletDamageScale = 1f;

	[ServerVar]
	public static float bulletAccuracy = 2f;

	[ServerVar]
	public static void drop(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			Debug.Log("heli called to : " + basePlayer.transform.position.ToString());
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.GetComponent<PatrolHelicopterAI>().SetInitialDestination(basePlayer.transform.position + new Vector3(0f, 10f, 0f), 0f);
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar]
	public static void calltome(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			Debug.Log("heli called to : " + basePlayer.transform.position.ToString());
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.GetComponent<PatrolHelicopterAI>().SetInitialDestination(basePlayer.transform.position + new Vector3(0f, 10f, 0f));
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar]
	public static void call(Arg arg)
	{
		if ((bool)ArgEx.Player(arg))
		{
			Debug.Log("Helicopter inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar]
	public static void strafe(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			RaycastHit hitInfo;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
			}
			else if (UnityEngine.Physics.Raycast(basePlayer.eyes.HeadRay(), out hitInfo, 1000f, 1218652417))
			{
				Debug.Log("strafing :" + hitInfo.point.ToString());
				heliInstance.interestZoneOrigin = hitInfo.point;
				heliInstance.ExitCurrentState();
				heliInstance.State_Strafe_Enter(hitInfo.point);
			}
			else
			{
				Debug.Log("strafe ray missed");
			}
		}
	}

	[ServerVar]
	public static void testpuzzle(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			_ = basePlayer.IsDeveloper;
		}
	}
}
