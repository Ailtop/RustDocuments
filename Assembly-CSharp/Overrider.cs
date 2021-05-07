using UnityEngine;

public class Overrider : MonoBehaviour
{
	[SerializeField]
	private MonoBehaviour _target;

	[SerializeField]
	[HideInInspector]
	private bool[] _overrides;

	[SerializeField]
	private object[] _objects = new object[3] { 1, 3, 5 };
}
