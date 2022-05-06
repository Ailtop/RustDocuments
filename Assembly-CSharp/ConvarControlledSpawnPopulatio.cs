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
	public bool IsWagon;

	private const float MIN_MARGIN = 40f;

	public override float TargetDensity => base.TargetDensity * (IsWagon ? ((float)TrainCar.wagons_per_engine) : 1f);

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
				if (length < 45f)
				{
					return false;
				}
				float distance = Random.Range(40f, length - 40f);
				newPos = trainTrackSpline.GetPointAndTangentCubicHermiteWorld(distance, out var tangent) + Vector3.up * 0.5f;
				newRot = Quaternion.LookRotation(tangent);
				return true;
			}
		}
		return false;
	}
}
