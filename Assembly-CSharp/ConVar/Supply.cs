using UnityEngine;

namespace ConVar
{
	[Factory("supply")]
	public class Supply : ConsoleSystem
	{
		private const string path = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";

		[ServerVar]
		public static void drop(Arg arg)
		{
			BasePlayer basePlayer = arg.Player();
			if ((bool)basePlayer)
			{
				Debug.Log("Supply Drop Inbound");
				BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
				if ((bool)baseEntity)
				{
					baseEntity.GetComponent<CargoPlane>().InitDropPosition(basePlayer.transform.position + new Vector3(0f, 10f, 0f));
					baseEntity.Spawn();
				}
			}
		}

		[ServerVar]
		public static void call(Arg arg)
		{
			if ((bool)arg.Player())
			{
				Debug.Log("Supply Drop Inbound");
				BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
				if ((bool)baseEntity)
				{
					baseEntity.Spawn();
				}
			}
		}
	}
}
