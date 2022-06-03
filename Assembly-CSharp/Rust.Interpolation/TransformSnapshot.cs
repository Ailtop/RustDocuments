using UnityEngine;

namespace Rust.Interpolation;

public struct TransformSnapshot : ISnapshot<TransformSnapshot>
{
	public Vector3 pos;

	public Quaternion rot;

	public float Time { get; set; }

	public TransformSnapshot(float time, Vector3 pos, Quaternion rot)
	{
		Time = time;
		this.pos = pos;
		this.rot = rot;
	}

	public void MatchValuesTo(TransformSnapshot entry)
	{
		pos = entry.pos;
		rot = entry.rot;
	}

	public void Lerp(TransformSnapshot prev, TransformSnapshot next, float delta)
	{
		pos = Vector3.LerpUnclamped(prev.pos, next.pos, delta);
		rot = Quaternion.SlerpUnclamped(prev.rot, next.rot, delta);
	}

	public TransformSnapshot GetNew()
	{
		return default(TransformSnapshot);
	}
}
