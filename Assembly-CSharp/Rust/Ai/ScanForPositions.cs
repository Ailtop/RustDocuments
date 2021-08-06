using System;
using Apex.AI;
using Apex.Serialization;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai
{
	[FriendlyName("Scan for Positions", "Scanning positions and storing them in the context")]
	public sealed class ScanForPositions : BaseAction
	{
		[ApexSerialization(defaultValue = 12f)]
		[FriendlyName("Sampling Range", "How large a range points are sampled in, in a square with the entity in the center")]
		public float SamplingRange = 12f;

		[ApexSerialization(defaultValue = 1.5f)]
		[FriendlyName("Sampling Density", "How much distance there is between individual samples")]
		public int SampleRings = 3;

		[ApexSerialization(defaultValue = false)]
		[FriendlyName("Calculate Path", "Calculating the path to each position ensures connectivity, but is expensive. Should be used for fallbacks/stuck-detection only?")]
		public bool CalculatePath;

		[ApexSerialization(defaultValue = false)]
		[FriendlyName("Percentage of Inner Circle for Calculate Path", "Calculating the path to each position ensures connectivity, but is expensive. Here we can define what percentage of the sampling range (it's inner circle) we want to calculate paths for.")]
		public float CalculatePathInnerCirclePercentageThreshold = 0.1f;

		[ApexSerialization]
		public bool ScanAllAreas = true;

		[ApexSerialization]
		public string AreaName;

		[ApexSerialization]
		public bool SampleTerrainHeight = true;

		private static NavMeshPath reusablePath = new NavMeshPath();

		public override void DoExecute(BaseContext c)
		{
			if (c.sampledPositions == null)
			{
				return;
			}
			if (c.sampledPositions.Count > 0)
			{
				c.sampledPositions.Clear();
			}
			Vector3 position = c.Position;
			float num = Time.time * 1f;
			float num2 = SamplingRange / (float)SampleRings;
			for (float num3 = SamplingRange; num3 > 0.5f; num3 -= num2)
			{
				num += 10f;
				for (float num4 = num % 35f; num4 < 360f; num4 += 35f)
				{
					Vector3 p = new Vector3(position.x + Mathf.Sin(num4 * ((float)Math.PI / 180f)) * num3, position.y, position.z + Mathf.Cos(num4 * ((float)Math.PI / 180f)) * num3);
					if (CalculatePath && num3 < SamplingRange * CalculatePathInnerCirclePercentageThreshold)
					{
						TryAddPoint(c, p, true, ScanAllAreas, AreaName, SampleTerrainHeight);
					}
					else
					{
						TryAddPoint(c, p, false, ScanAllAreas, AreaName, SampleTerrainHeight);
					}
				}
			}
		}

		private static void TryAddPoint(BaseContext c, Vector3 p, bool calculatePath, bool scanAllAreas, string areaName, bool sampleTerrainHeight)
		{
			int areaMask = ((scanAllAreas || string.IsNullOrEmpty(areaName)) ? (-1) : (1 << NavMesh.GetAreaFromName(areaName)));
			if (sampleTerrainHeight)
			{
				p.y = TerrainMeta.HeightMap.GetHeight(p);
			}
			NavMeshHit hit;
			if (!NavMesh.SamplePosition(p, out hit, 4f, areaMask) || !hit.hit)
			{
				return;
			}
			if (calculatePath || c.AIAgent.IsStuck)
			{
				if (NavMesh.CalculatePath(hit.position, c.Position, areaMask, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete)
				{
					c.sampledPositions.Add(hit.position);
				}
			}
			else
			{
				c.sampledPositions.Add(hit.position);
			}
		}
	}
}
