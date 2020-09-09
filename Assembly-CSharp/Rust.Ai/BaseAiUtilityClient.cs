using Apex.AI;
using Apex.AI.Components;
using System;

namespace Rust.Ai
{
	public class BaseAiUtilityClient : UtilityAIClient
	{
		public BaseAiUtilityClient(Guid aiId, IContextProvider contextProvider)
			: base(aiId, contextProvider)
		{
		}

		public BaseAiUtilityClient(IUtilityAI ai, IContextProvider contextProvider)
			: base(ai, contextProvider)
		{
		}

		protected override void OnPause()
		{
		}

		protected override void OnResume()
		{
		}

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}

		public void Initialize()
		{
			Start();
		}

		public void Kill()
		{
			Stop();
		}
	}
}
