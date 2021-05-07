using UnityEngine;

public enum FreeParallaxPositionMode
{
	[Tooltip("Wrap and anchor to the top (or right for a vertical parallax) of the screen")]
	WrapAnchorTop,
	[Tooltip("Wrap and anchor to the bottom (or left for a vertical parallax) of the screen")]
	WrapAnchorBottom,
	[Tooltip("No wrap, this is an individual object that starts off screen")]
	IndividualStartOffScreen,
	[Tooltip("No wrap, this is an individual object that starts on screen")]
	IndividualStartOnScreen,
	[Tooltip("Wrap and maintain original position")]
	WrapAnchorNone
}
