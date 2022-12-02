using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class BuildingProximity : PrefabAttribute
{
	public struct ProximityInfo
	{
		public bool hit;

		public bool connection;

		public Line line;

		public float sqrDist;
	}

	private const float check_radius = 2f;

	private const float check_forgiveness = 0.01f;

	private const float foundation_width = 3f;

	private const float foundation_extents = 1.5f;

	public static bool Check(BasePlayer player, Construction construction, Vector3 position, Quaternion rotation)
	{
		OBB oBB = new OBB(position, rotation, construction.bounds);
		float radius = oBB.extents.magnitude + 2f;
		List<BuildingBlock> obj = Pool.GetList<BuildingBlock>();
		Vis.Entities(oBB.position, radius, obj, 2097152);
		uint num = 0u;
		for (int i = 0; i < obj.Count; i++)
		{
			BuildingBlock buildingBlock = obj[i];
			Construction blockDefinition = buildingBlock.blockDefinition;
			Vector3 position2 = buildingBlock.transform.position;
			Quaternion rotation2 = buildingBlock.transform.rotation;
			ProximityInfo proximity = GetProximity(construction, position, rotation, blockDefinition, position2, rotation2);
			ProximityInfo proximity2 = GetProximity(blockDefinition, position2, rotation2, construction, position, rotation);
			ProximityInfo proximityInfo = default(ProximityInfo);
			proximityInfo.hit = proximity.hit || proximity2.hit;
			proximityInfo.connection = proximity.connection || proximity2.connection;
			if (proximity.sqrDist <= proximity2.sqrDist)
			{
				proximityInfo.line = proximity.line;
				proximityInfo.sqrDist = proximity.sqrDist;
			}
			else
			{
				proximityInfo.line = proximity2.line;
				proximityInfo.sqrDist = proximity2.sqrDist;
			}
			if (proximityInfo.connection)
			{
				BuildingManager.Building building = buildingBlock.GetBuilding();
				if (building != null)
				{
					BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
					if (dominatingBuildingPrivilege != null)
					{
						if (!construction.canBypassBuildingPermission && !dominatingBuildingPrivilege.IsAuthed(player))
						{
							Construction.lastPlacementError = "Cannot attach to unauthorized building";
							Pool.FreeList(ref obj);
							return true;
						}
						if (num == 0)
						{
							num = building.ID;
						}
						else if (num != building.ID)
						{
							if (!dominatingBuildingPrivilege.IsAuthed(player))
							{
								Construction.lastPlacementError = "Cannot attach to unauthorized building";
							}
							else
							{
								Construction.lastPlacementError = "Cannot connect two buildings with cupboards";
							}
							Pool.FreeList(ref obj);
							return true;
						}
					}
				}
			}
			if (proximityInfo.hit)
			{
				Vector3 v = proximityInfo.line.point1 - proximityInfo.line.point0;
				if (!(Mathf.Abs(v.y) > 1.49f) && !(v.Magnitude2D() > 1.49f))
				{
					Construction.lastPlacementError = "Too close to another building";
					Pool.FreeList(ref obj);
					return true;
				}
			}
		}
		Pool.FreeList(ref obj);
		return false;
	}

	public static ProximityInfo GetProximity(Construction construction1, Vector3 position1, Quaternion rotation1, Construction construction2, Vector3 position2, Quaternion rotation2)
	{
		ProximityInfo result = default(ProximityInfo);
		result.hit = false;
		result.connection = false;
		result.line = default(Line);
		result.sqrDist = float.MaxValue;
		for (int i = 0; i < construction1.allSockets.Length; i++)
		{
			ConstructionSocket constructionSocket = construction1.allSockets[i] as ConstructionSocket;
			if (constructionSocket == null)
			{
				continue;
			}
			for (int j = 0; j < construction2.allSockets.Length; j++)
			{
				Socket_Base socket = construction2.allSockets[j];
				if (constructionSocket.CanConnect(position1, rotation1, socket, position2, rotation2))
				{
					result.connection = true;
					return result;
				}
			}
		}
		if (!result.hit)
		{
			for (int k = 0; k < construction1.allSockets.Length; k++)
			{
				NeighbourSocket neighbourSocket = construction1.allSockets[k] as NeighbourSocket;
				if (neighbourSocket == null)
				{
					continue;
				}
				for (int l = 0; l < construction2.allSockets.Length; l++)
				{
					Socket_Base socket2 = construction2.allSockets[l];
					if (neighbourSocket.CanConnect(position1, rotation1, socket2, position2, rotation2))
					{
						result.connection = true;
						return result;
					}
				}
			}
		}
		if (!result.connection && construction1.allProximities.Length != 0)
		{
			for (int m = 0; m < construction1.allSockets.Length; m++)
			{
				ConstructionSocket constructionSocket2 = construction1.allSockets[m] as ConstructionSocket;
				if (constructionSocket2 == null || constructionSocket2.socketType != ConstructionSocket.Type.Wall)
				{
					continue;
				}
				Vector3 selectPivot = constructionSocket2.GetSelectPivot(position1, rotation1);
				for (int n = 0; n < construction2.allProximities.Length; n++)
				{
					Vector3 selectPivot2 = construction2.allProximities[n].GetSelectPivot(position2, rotation2);
					Line line = new Line(selectPivot, selectPivot2);
					float sqrMagnitude = (line.point1 - line.point0).sqrMagnitude;
					if (sqrMagnitude < result.sqrDist)
					{
						result.hit = true;
						result.line = line;
						result.sqrDist = sqrMagnitude;
					}
				}
			}
		}
		return result;
	}

	public Vector3 GetSelectPivot(Vector3 position, Quaternion rotation)
	{
		return position + rotation * worldPosition;
	}

	protected override Type GetIndexedType()
	{
		return typeof(BuildingProximity);
	}
}
