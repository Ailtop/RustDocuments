using UnityEngine;

public class uiPlayerPreview : SingletonComponent<uiPlayerPreview>
{
	public enum EffectMode
	{
		Poster = 0,
		Polaroid = 1
	}

	public Camera previewCamera;

	public PlayerModel playermodel;

	public GameObject wantedSnapshotEffectPosterRoot;

	public GameObject wantedSnapshotEffectPolaroidRoot;

	public SegmentMaskPositioning segmentMask;
}
