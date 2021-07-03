using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class WiggleRenderer : PostProcessEffectRenderer<Wiggle>
{
	private int timerProperty = Shader.PropertyToID("_timer");

	private int scaleProperty = Shader.PropertyToID("_scale");

	private Shader wiggleShader;

	private float timer;

	public override void Init()
	{
		base.Init();
		wiggleShader = Shader.Find("Hidden/PostProcessing/Wiggle");
	}

	public override void Render(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Wiggle");
		timer += base.settings.speed.value * Time.deltaTime;
		PropertySheet propertySheet = context.propertySheets.Get(wiggleShader);
		propertySheet.properties.Clear();
		propertySheet.properties.SetFloat(timerProperty, timer);
		propertySheet.properties.SetFloat(scaleProperty, base.settings.scale.value);
		RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, propertySheet, 0);
		command.EndSample("Wiggle");
	}
}
