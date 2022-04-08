using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

public class UIPrimitiveBase : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
{
	protected static Material s_ETC1DefaultUI;

	private List<Vector2> outputList = new List<Vector2>();

	[SerializeField]
	private Sprite m_Sprite;

	[NonSerialized]
	private Sprite m_OverrideSprite;

	internal float m_EventAlphaThreshold = 1f;

	[SerializeField]
	private ResolutionMode m_improveResolution;

	[SerializeField]
	protected float m_Resolution;

	[SerializeField]
	private bool m_useNativeSize;

	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_Sprite, value))
			{
				GeneratedUVs();
			}
			SetAllDirty();
		}
	}

	public Sprite overrideSprite
	{
		get
		{
			return activeSprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value))
			{
				GeneratedUVs();
			}
			SetAllDirty();
		}
	}

	protected Sprite activeSprite
	{
		get
		{
			if (!(m_OverrideSprite != null))
			{
				return sprite;
			}
			return m_OverrideSprite;
		}
	}

	public float eventAlphaThreshold
	{
		get
		{
			return m_EventAlphaThreshold;
		}
		set
		{
			m_EventAlphaThreshold = value;
		}
	}

	public ResolutionMode ImproveResolution
	{
		get
		{
			return m_improveResolution;
		}
		set
		{
			m_improveResolution = value;
			SetAllDirty();
		}
	}

	public float Resoloution
	{
		get
		{
			return m_Resolution;
		}
		set
		{
			m_Resolution = value;
			SetAllDirty();
		}
	}

	public bool UseNativeSize
	{
		get
		{
			return m_useNativeSize;
		}
		set
		{
			m_useNativeSize = value;
			SetAllDirty();
		}
	}

	public static Material defaultETC1GraphicMaterial
	{
		get
		{
			if (s_ETC1DefaultUI == null)
			{
				s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
			}
			return s_ETC1DefaultUI;
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if (activeSprite == null)
			{
				if (material != null && material.mainTexture != null)
				{
					return material.mainTexture;
				}
				return Graphic.s_WhiteTexture;
			}
			return activeSprite.texture;
		}
	}

	public bool hasBorder
	{
		get
		{
			if (activeSprite != null)
			{
				return activeSprite.border.sqrMagnitude > 0f;
			}
			return false;
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			float num = 100f;
			if ((bool)activeSprite)
			{
				num = activeSprite.pixelsPerUnit;
			}
			float num2 = 100f;
			if ((bool)base.canvas)
			{
				num2 = base.canvas.referencePixelsPerUnit;
			}
			return num / num2;
		}
	}

	public override Material material
	{
		get
		{
			if (m_Material != null)
			{
				return m_Material;
			}
			if ((bool)activeSprite && activeSprite.associatedAlphaSplitTexture != null)
			{
				return defaultETC1GraphicMaterial;
			}
			return defaultMaterial;
		}
		set
		{
			base.material = value;
		}
	}

	public virtual float minWidth => 0f;

	public virtual float preferredWidth
	{
		get
		{
			if (overrideSprite == null)
			{
				return 0f;
			}
			return overrideSprite.rect.size.x / pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => 0f;

	public virtual float preferredHeight
	{
		get
		{
			if (overrideSprite == null)
			{
				return 0f;
			}
			return overrideSprite.rect.size.y / pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => 0;

	protected UIPrimitiveBase()
	{
		base.useLegacyMeshGeneration = false;
	}

	protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
	{
		UIVertex[] array = new UIVertex[4];
		for (int i = 0; i < vertices.Length; i++)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = color;
			simpleVert.position = vertices[i];
			simpleVert.uv0 = uvs[i];
			array[i] = simpleVert;
		}
		return array;
	}

	protected Vector2[] IncreaseResolution(Vector2[] input)
	{
		return IncreaseResolution(new List<Vector2>(input)).ToArray();
	}

	protected List<Vector2> IncreaseResolution(List<Vector2> input)
	{
		outputList.Clear();
		switch (ImproveResolution)
		{
		case ResolutionMode.PerLine:
		{
			float num3 = 0f;
			float num = 0f;
			for (int j = 0; j < input.Count - 1; j++)
			{
				num3 += Vector2.Distance(input[j], input[j + 1]);
			}
			ResolutionToNativeSize(num3);
			num = num3 / m_Resolution;
			int num4 = 0;
			for (int k = 0; k < input.Count - 1; k++)
			{
				Vector2 vector3 = input[k];
				outputList.Add(vector3);
				Vector2 vector4 = input[k + 1];
				float num5 = Vector2.Distance(vector3, vector4) / num;
				float num6 = 1f / num5;
				for (int l = 0; (float)l < num5; l++)
				{
					outputList.Add(Vector2.Lerp(vector3, vector4, (float)l * num6));
					num4++;
				}
				outputList.Add(vector4);
			}
			break;
		}
		case ResolutionMode.PerSegment:
		{
			for (int i = 0; i < input.Count - 1; i++)
			{
				Vector2 vector = input[i];
				outputList.Add(vector);
				Vector2 vector2 = input[i + 1];
				ResolutionToNativeSize(Vector2.Distance(vector, vector2));
				float num = 1f / m_Resolution;
				for (float num2 = 1f; num2 < m_Resolution; num2 += 1f)
				{
					outputList.Add(Vector2.Lerp(vector, vector2, num * num2));
				}
				outputList.Add(vector2);
			}
			break;
		}
		}
		return outputList;
	}

	protected virtual void GeneratedUVs()
	{
	}

	protected virtual void ResolutionToNativeSize(float distance)
	{
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (m_EventAlphaThreshold >= 1f)
		{
			return true;
		}
		Sprite sprite = overrideSprite;
		if (sprite == null)
		{
			return true;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var localPoint);
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		localPoint.x += base.rectTransform.pivot.x * pixelAdjustedRect.width;
		localPoint.y += base.rectTransform.pivot.y * pixelAdjustedRect.height;
		localPoint = MapCoordinate(localPoint, pixelAdjustedRect);
		Rect textureRect = sprite.textureRect;
		Vector2 vector = new Vector2(localPoint.x / textureRect.width, localPoint.y / textureRect.height);
		float u = Mathf.Lerp(textureRect.x, textureRect.xMax, vector.x) / (float)sprite.texture.width;
		float v = Mathf.Lerp(textureRect.y, textureRect.yMax, vector.y) / (float)sprite.texture.height;
		try
		{
			return sprite.texture.GetPixelBilinear(u, v).a >= m_EventAlphaThreshold;
		}
		catch (UnityException ex)
		{
			Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
			return true;
		}
	}

	private Vector2 MapCoordinate(Vector2 local, Rect rect)
	{
		_ = sprite.rect;
		return new Vector2(local.x * rect.width, local.y * rect.height);
	}

	private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int i = 0; i <= 1; i++)
		{
			float num = border[i] + border[i + 2];
			if (rect.size[i] < num && num != 0f)
			{
				float num2 = rect.size[i] / num;
				border[i] *= num2;
				border[i + 2] *= num2;
			}
		}
		return border;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetAllDirty();
	}
}
