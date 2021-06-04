using UnityEngine;

public class LandmarkInfo : MonoBehaviour
{
	[Header("LandmarkInfo")]
	public bool shouldDisplayOnMap;

	public Translate.Phrase displayPhrase;

	public Sprite mapIcon;

	protected virtual void Awake()
	{
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.Landmarks.Add(this);
		}
	}
}
