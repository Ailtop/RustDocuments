using UnityEngine;

public class JunkPileWater : JunkPile
{
	public class JunkpileWaterWorkQueue : ObjectWorkQueue<JunkPileWater>
	{
		protected override void RunJob(JunkPileWater entity)
		{
			if (ShouldAdd(entity))
			{
				entity.UpdateNearbyPlayers();
			}
		}

		protected override bool ShouldAdd(JunkPileWater entity)
		{
			if (base.ShouldAdd(entity))
			{
				return BaseNetworkableEx.IsValid(entity);
			}
			return false;
		}
	}

	public static JunkpileWaterWorkQueue junkpileWaterWorkQueue = new JunkpileWaterWorkQueue();

	[ServerVar]
	[Help("How many milliseconds to budget for processing life story updates per frame")]
	public static float framebudgetms = 0.25f;

	public Transform[] buoyancyPoints;

	public bool debugDraw;

	private Quaternion baseRotation = Quaternion.identity;

	private bool first = true;

	private TimeUntil nextPlayerCheck;

	private bool hasPlayersNearby;

	public override void Spawn()
	{
		Vector3 position = base.transform.position;
		position.y = TerrainMeta.WaterMap.GetHeight(base.transform.position);
		base.transform.position = position;
		base.Spawn();
		baseRotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
	}

	public void FixedUpdate()
	{
		if (!base.isClient)
		{
			UpdateMovement();
		}
	}

	public void UpdateMovement()
	{
		if ((float)nextPlayerCheck <= 0f)
		{
			nextPlayerCheck = Random.Range(0.5f, 1f);
			junkpileWaterWorkQueue.Add(this);
		}
		if (isSinking || !hasPlayersNearby)
		{
			return;
		}
		float height = WaterSystem.GetHeight(base.transform.position);
		base.transform.position = new Vector3(base.transform.position.x, height, base.transform.position.z);
		if (buoyancyPoints != null && buoyancyPoints.Length >= 3)
		{
			Vector3 position = base.transform.position;
			Vector3 localPosition = buoyancyPoints[0].localPosition;
			Vector3 localPosition2 = buoyancyPoints[1].localPosition;
			Vector3 localPosition3 = buoyancyPoints[2].localPosition;
			Vector3 vector = localPosition + position;
			Vector3 vector2 = localPosition2 + position;
			Vector3 vector3 = localPosition3 + position;
			vector.y = WaterSystem.GetHeight(vector);
			vector2.y = WaterSystem.GetHeight(vector2);
			vector3.y = WaterSystem.GetHeight(vector3);
			Vector3 position2 = new Vector3(position.x, vector.y - localPosition.y, position.z);
			Vector3 rhs = vector2 - vector;
			Vector3 vector4 = Vector3.Cross(vector3 - vector, rhs);
			Vector3 eulerAngles = Quaternion.LookRotation(new Vector3(vector4.x, vector4.z, vector4.y)).eulerAngles;
			Quaternion quaternion = Quaternion.Euler(0f - eulerAngles.x, 0f, 0f - eulerAngles.y);
			if (first)
			{
				baseRotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
				first = false;
			}
			base.transform.SetPositionAndRotation(position2, quaternion * baseRotation);
		}
	}

	public void UpdateNearbyPlayers()
	{
		hasPlayersNearby = BaseNetworkable.HasCloseConnections(base.transform.position, 16f);
	}
}
