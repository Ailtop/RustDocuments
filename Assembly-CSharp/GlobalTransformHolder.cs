using UnityEngine;

public class GlobalTransformHolder : MonoBehaviour
{
	private Transform[] _children;

	private Vector3[] _originalPositions;

	private void Awake()
	{
		int childCount = base.transform.childCount;
		_children = new Transform[childCount];
		_originalPositions = new Vector3[childCount];
		for (int i = 0; i < childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			_children[i] = child;
			_originalPositions[i] = child.localPosition;
			child.parent = null;
		}
		base.transform.DetachChildren();
	}

	private void OnDestroy()
	{
		Transform[] children = _children;
		for (int i = 0; i < children.Length; i++)
		{
			Object.Destroy(children[i].gameObject);
		}
	}

	public void ResetChildrenToLocal()
	{
		for (int i = 0; i < _originalPositions.Length; i++)
		{
			_children[i].position = base.transform.TransformPoint(_originalPositions[i]);
		}
	}
}
