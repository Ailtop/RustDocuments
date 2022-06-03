#define UNITY_ASSERTIONS
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "Rust/Convar Controlled Spawn Population")]
public class ConvarControlledSpawnPopulation : SpawnPopulation
{
	[Header("Convars")]
	public string PopulationConvar;

	private ConsoleSystem.Command _command;

	protected ConsoleSystem.Command Command
	{
		get
		{
			if (_command == null)
			{
				_command = ConsoleSystem.Index.Server.Find(PopulationConvar);
				Assert.IsNotNull(_command, $"{this} has missing convar {PopulationConvar}");
			}
			return _command;
		}
	}

	public override float TargetDensity => Command.AsFloat;
}
public class ConvarControlledSpawnPopulationRailRing : ConvarControlledSpawnPopulation
{
	public enum TrainCarType
	{
		WorkCart = 0,
		WorkCartWithCover = 1,
		Wagon = 2
	}

	public TrainCarType trainCarType;

	private const float MIN_MARGIN = 60f;

	public override float TargetDensity => trainCarType switch
	{
		TrainCarType.WorkCart => base.TargetDensity * (1f - TrainCar.variant_ratio), 
		TrainCarType.WorkCartWithCover => base.TargetDensity * TrainCar.variant_ratio, 
		TrainCarType.Wagon => base.TargetDensity * (float)TrainCar.wagons_per_engine * 1.1f, 
		_ => base.TargetDensity, 
	};

	public override bool OverrideSpawnPosition(ref Vector3 newPos, ref Quaternion newRot)
	{
		if (TrainTrackSpline.SidingSplines.Count <= 0)
		{
			return false;
		}
		int num = 0;
		while (num < 50)
		{
			num++;
			int index = Random.Range(0, TrainTrackSpline.SidingSplines.Count);
			if (TrainTrackSpline.SidingSplines[index] != null)
			{
				TrainTrackSpline trainTrackSpline = TrainTrackSpline.SidingSplines[index];
				float length = trainTrackSpline.GetLength();
				if (length < 65f)
				{
					return false;
				}
				float distance = Random.Range(60f, length - 60f);
				newPos = trainTrackSpline.GetPointAndTangentCubicHermiteWorld(distance, out var tangent) + Vector3.up * 0.5f;
				newRot = Quaternion.LookRotation(tangent);
				return true;
			}
		}
		return false;
	}
}
