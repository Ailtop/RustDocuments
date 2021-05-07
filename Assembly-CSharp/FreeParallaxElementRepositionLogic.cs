using System;
using UnityEngine;

[Serializable]
public class FreeParallaxElementRepositionLogic
{
	[Tooltip("Set whether to wrap the object (for a full width (or height for vertical parallax) element, or to use individual elements such as trees, clouds and light rays.")]
	public FreeParallaxPositionMode PositionMode = FreeParallaxPositionMode.WrapAnchorBottom;

	[Tooltip("Set to a percentage of the screen height (or width for vertical parallax) to scale the element. Set to 0 to leave as the original scale, or 1 to scale to the height (or width for a vertical parallax) of the screen.")]
	public float ScaleHeight;

	[Tooltip("Sorting order for rendering. Leave as 0 to use original sort order.")]
	public int SortingOrder;

	[Tooltip("Minimum y percent in viewport space to reposition when object leaves the screen. 0.5 would position it at least half way up the screen for a horizontal parallax, or 5 would position it at least 5 screen heights away for a vertical parallax.")]
	public float MinYPercent;

	[Tooltip("Maximum y percent in viewport space to reposition when object leaves the screen. 0.75 would position it no more than 3/4 up the screen for a horizontal parallax, or 10 would position it no more than 10 screen heights away for a vertical parallax.")]
	public float MaxYPercent;

	[Tooltip("Minimum x percent in viewport space to reposition when object leaves the screen. 5 would position it at least 5 screen widths away for a horizontal parallax, or 0.5 would position it at least half way across the screen for a vertical parallax.")]
	public float MinXPercent;

	[Tooltip("Maximum x percent in viewport space to reposition when object leaves the screen. 10 would position it no more than 10 screen widths away for a horizontal parallax or 0.75 would position it no more than 3/4 across the screen for a vertical parallax.")]
	public float MaxXPercent;
}
