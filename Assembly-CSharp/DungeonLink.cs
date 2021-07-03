using UnityEngine;

public class DungeonLink : MonoBehaviour
{
	public Transform UpSocket;

	public Transform DownSocket;

	public DungeonLinkType UpType;

	public DungeonLinkType DownType;

	public int Priority;

	public int Rotation;

	protected void Start()
	{
		if (!(TerrainMeta.Path == null))
		{
			DungeonInfo dungeonInfo = TerrainMeta.Path.FindClosest(TerrainMeta.Path.DungeonEntrances, base.transform.position);
			if (!(dungeonInfo == null))
			{
				dungeonInfo.Links.Add(base.gameObject);
			}
		}
	}
}
