namespace Rust.Ai.HTN.Sensors
{
	public interface INpcSensor
	{
		float TickFrequency
		{
			get;
			set;
		}

		float LastTickTime
		{
			get;
			set;
		}

		void Tick(IHTNAgent npc, float deltaTime, float time);
	}
}
