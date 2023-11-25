using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("texture")]
public class Texture : ConsoleSystem
{
	[ClientVar]
	public static int streamingBudgetOverride;

	[ClientVar(Saved = true, Help = "Enable/Disable texture streaming")]
	public static bool streaming
	{
		get
		{
			return QualitySettings.streamingMipmapsActive;
		}
		set
		{
			QualitySettings.streamingMipmapsActive = value;
		}
	}

	[ClientVar]
	public static void stats(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Supports streaming:               {SystemInfo.supportsMipStreaming}");
		stringBuilder.AppendLine($"Streaming enabled:                {QualitySettings.streamingMipmapsActive}");
		stringBuilder.AppendLine($"Immediately discard unused mips:  {UnityEngine.Texture.streamingTextureDiscardUnusedMips}");
		stringBuilder.AppendLine($"Max level of reduction:           {QualitySettings.streamingMipmapsMaxLevelReduction}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"currentTextureMemory:             {UnityEngine.Texture.currentTextureMemory / 1048576}MB (current estimated usage)");
		stringBuilder.AppendLine($"desiredTextureMemory:             {UnityEngine.Texture.desiredTextureMemory / 1048576}MB");
		stringBuilder.AppendLine($"nonStreamingTextureCount:         {UnityEngine.Texture.nonStreamingTextureCount}");
		stringBuilder.AppendLine($"nonStreamingTextureMemory:        {UnityEngine.Texture.nonStreamingTextureMemory / 1048576}MB");
		stringBuilder.AppendLine($"streamingTextureCount:            {UnityEngine.Texture.streamingTextureCount}");
		stringBuilder.AppendLine($"targetTextureMemory:              {UnityEngine.Texture.targetTextureMemory / 1048576}MB");
		stringBuilder.AppendLine($"totalTextureMemory:               {UnityEngine.Texture.totalTextureMemory / 1048576}MB (if everything was loaded at highest quality)");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"streamingMipmapUploadCount:       {UnityEngine.Texture.streamingMipmapUploadCount}");
		stringBuilder.AppendLine($"streamingTextureLoadingCount:     {UnityEngine.Texture.streamingTextureLoadingCount}");
		stringBuilder.AppendLine($"streamingTexturePendingLoadCount: {UnityEngine.Texture.streamingTexturePendingLoadCount}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"TargetBudget:                     {QualitySettings.streamingMipmapsMemoryBudget}MB");
		arg.ReplyWith(stringBuilder.ToString());
	}
}
