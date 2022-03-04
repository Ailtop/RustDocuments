using System;

public class ParticleCollisionLOD : LODComponentParticleSystem
{
	public enum QualityLevel
	{
		Disabled = -1,
		HighQuality = 0,
		MediumQuality = 1,
		LowQuality = 2
	}

	[Serializable]
	public class State
	{
		public float distance;

		public QualityLevel quality = QualityLevel.Disabled;
	}

	[Horizontal(1, 0)]
	public State[] States;
}
