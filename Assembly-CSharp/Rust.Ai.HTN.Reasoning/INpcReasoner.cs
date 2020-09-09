namespace Rust.Ai.HTN.Reasoning
{
	public interface INpcReasoner
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
