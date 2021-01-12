using UnityEngine;

public class VehicleTerrainHandler
{
	public string CurGroundPhysicsMatName;

	public bool IsOnRoad;

	public bool IsOnSnowOrIce;

	public bool IsOnSand;

	public bool IsOnIce;

	public bool IsGrounded;

	public float RayLength = 1.5f;

	private readonly string[] TerrainRoad = new string[5]
	{
		"rock",
		"concrete",
		"gravel",
		"metal",
		"path"
	};

	private const float SECONDS_BETWEEN_TERRAIN_SAMPLE = 0.25f;

	private TimeSince timeSinceTerrainCheck;

	private readonly BaseVehicle vehicle;

	public VehicleTerrainHandler(BaseVehicle vehicle)
	{
		this.vehicle = vehicle;
	}

	public void FixedUpdate()
	{
		if (!vehicle.IsStationary() && (float)timeSinceTerrainCheck > 0.25f)
		{
			DoTerrainCheck();
		}
	}

	private void DoTerrainCheck()
	{
		timeSinceTerrainCheck = Random.Range(-0.025f, 0.025f);
		Transform transform = vehicle.transform;
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position + transform.up * 0.5f, -transform.up, out hitInfo, RayLength, 27328512, QueryTriggerInteraction.Ignore))
		{
			CurGroundPhysicsMatName = AssetNameCache.GetNameLower(ColliderEx.GetMaterialAt(hitInfo.collider, hitInfo.point));
			IsOnSnowOrIce = (CurGroundPhysicsMatName == "snow");
			IsOnSand = (CurGroundPhysicsMatName == "sand");
			IsOnIce = (IsOnSnowOrIce && hitInfo.collider.name.ToLower().Contains("ice"));
			IsGrounded = true;
		}
		else
		{
			CurGroundPhysicsMatName = "concrete";
			IsOnSnowOrIce = false;
			IsOnSand = false;
			IsOnIce = false;
			IsGrounded = false;
		}
		IsOnRoad = GetOnRoad(CurGroundPhysicsMatName);
	}

	private bool GetOnRoad(string physicMat)
	{
		for (int i = 0; i < TerrainRoad.Length; i++)
		{
			if (TerrainRoad[i] == physicMat)
			{
				return true;
			}
		}
		return false;
	}
}
