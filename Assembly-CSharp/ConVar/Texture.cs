using UnityEngine;

namespace ConVar
{
	[Factory("texture")]
	public class Texture : ConsoleSystem
	{
		[ClientVar]
		public static void stats(Arg arg)
		{
			string text = "currentTextureMemory: " + UnityEngine.Texture.currentTextureMemory / 1048576uL + " MB\ndesiredTextureMemory: " + UnityEngine.Texture.desiredTextureMemory / 1048576uL + " MB\nnonStreamingTextureCount: " + UnityEngine.Texture.nonStreamingTextureCount + "\nnonStreamingTextureMemory: " + UnityEngine.Texture.nonStreamingTextureMemory / 1048576uL + " MB\nstreamingTextureCount: " + UnityEngine.Texture.streamingTextureCount + "\ntargetTextureMemory: " + UnityEngine.Texture.targetTextureMemory / 1048576uL + " MB\ntotalTextureMemory: " + UnityEngine.Texture.totalTextureMemory / 1048576uL + " MB\n";
			Debug.Log("TargetBudget: " + QualitySettings.streamingMipmapsMemoryBudget + " MB, ActualBudget: " + QualitySettings.streamingMipmapsMemoryBudget + " MB\n" + text);
		}
	}
}
