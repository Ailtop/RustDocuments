using System;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[Preserve]
public sealed class FastApproximateAntialiasing
{
	[FormerlySerializedAs("mobileOptimized")]
	[Tooltip("Boost performances by lowering the effect quality. This setting is meant to be used on mobile and other low-end platforms but can also provide a nice performance boost on desktops and consoles.")]
	public bool fastMode;

	[Tooltip("Keep alpha channel. This will slightly lower the effect quality but allows rendering against a transparent background.")]
	public bool keepAlpha;
}
