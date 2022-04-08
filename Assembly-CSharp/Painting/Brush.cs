using System;
using UnityEngine;

namespace Painting;

[Serializable]
public class Brush
{
	public float spacing;

	public Vector2 brushSize;

	public Texture2D texture;

	public Color color;

	public bool erase;
}
