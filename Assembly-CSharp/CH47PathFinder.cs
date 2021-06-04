using System.Collections.Generic;
using UnityEngine;

public class CH47PathFinder : BasePathFinder
{
	public List<Vector3> visitedPatrolPoints = new List<Vector3>();

	public override Vector3 GetRandomPatrolPoint()
	{
		Vector3 vector = Vector3.zero;
		MonumentInfo monumentInfo = null;
		if (TerrainMeta.Path != null && TerrainMeta.Path.Monuments != null && TerrainMeta.Path.Monuments.Count > 0)
		{
			int count = TerrainMeta.Path.Monuments.Count;
			int num = Random.Range(0, count);
			for (int i = 0; i < count; i++)
			{
				int num2 = i + num;
				if (num2 >= count)
				{
					num2 -= count;
				}
				MonumentInfo monumentInfo2 = TerrainMeta.Path.Monuments[num2];
				if (monumentInfo2.Type == MonumentType.Cave || monumentInfo2.Type == MonumentType.WaterWell || monumentInfo2.Tier == MonumentTier.Tier0 || (monumentInfo2.Tier & MonumentTier.Tier0) > (MonumentTier)0)
				{
					continue;
				}
				bool flag = false;
				foreach (Vector3 visitedPatrolPoint in visitedPatrolPoints)
				{
					if (Vector3Ex.Distance2D(monumentInfo2.transform.position, visitedPatrolPoint) < 100f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					monumentInfo = monumentInfo2;
					break;
				}
			}
			if (monumentInfo == null)
			{
				visitedPatrolPoints.Clear();
				monumentInfo = GetRandomValidMonumentInfo();
			}
		}
		if (monumentInfo != null)
		{
			visitedPatrolPoints.Add(monumentInfo.transform.position);
			vector = monumentInfo.transform.position;
		}
		else
		{
			float x = TerrainMeta.Size.x;
			float y = 30f;
			vector = Vector3Ex.Range(-1f, 1f);
			vector.y = 0f;
			vector.Normalize();
			vector *= x * Random.Range(0f, 0.75f);
			vector.y = y;
		}
		float num3 = Mathf.Max(TerrainMeta.WaterMap.GetHeight(vector), TerrainMeta.HeightMap.GetHeight(vector));
		float num4 = num3;
		RaycastHit hitInfo;
		if (Physics.SphereCast(vector + new Vector3(0f, 200f, 0f), 20f, Vector3.down, out hitInfo, 300f, 1218511105))
		{
			num4 = Mathf.Max(hitInfo.point.y, num3);
		}
		vector.y = num4 + 30f;
		return vector;
	}

	private MonumentInfo GetRandomValidMonumentInfo()
	{
		int count = TerrainMeta.Path.Monuments.Count;
		int num = Random.Range(0, count);
		for (int i = 0; i < count; i++)
		{
			int num2 = i + num;
			if (num2 >= count)
			{
				num2 -= count;
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.Monuments[num2];
			if (monumentInfo.Type != 0 && monumentInfo.Type != MonumentType.WaterWell && monumentInfo.Tier != MonumentTier.Tier0)
			{
				return monumentInfo;
			}
		}
		return null;
	}
}
