using UnityEngine;

public class FixedParallax : MonoBehaviour
{
	[SerializeField]
	[Tooltip("카메라 위치로 인해 변경되는 오브젝트의 중심점을 움직입니다.\n카메라의 중심이 정확히 이 오브젝트의 위치 + offset에 위치할 때 이 오브젝트가 에디터상에서 배치한 그 위치에 보여집니다.")]
	private Vector2 _offset;

	[SerializeField]
	[Tooltip("카메라로부터 떨어진 거리에 이 값을 곱한 만큼 움직입니다. 0이면 움직이지 않고, 1이면 항상 카메라에 붙어 다니게 됩니다. 값의 범위에 제한은 없습니다.")]
	private Vector2 _positionRatio;

	private Vector3 _origin;

	private void Awake()
	{
		_origin = base.transform.position;
	}

	private void LateUpdate()
	{
		SetPosition();
	}

	private void SetPosition()
	{
		Vector2 vector = _origin;
		Vector2 vector2 = (Camera.main.transform.position - (_origin + (Vector3)_offset)) * _positionRatio;
		vector += vector2;
		base.transform.position = vector;
	}
}
