using System.Collections.Generic;
using UnityEngine;

public class PowerLineWireSpan : MonoBehaviour
{
	public GameObjectRef wirePrefab;

	public Transform start;

	public Transform end;

	public float WireLength;

	public List<PowerLineWireConnection> connections = new List<PowerLineWireConnection>();

	public void Init(PowerLineWire wire)
	{
		if ((bool)start && (bool)end)
		{
			WireLength = Vector3.Distance(start.position, end.position);
			for (int i = 0; i < connections.Count; i++)
			{
				Vector3 a = start.TransformPoint(connections[i].outOffset);
				Vector3 vector = end.TransformPoint(connections[i].inOffset);
				WireLength = (a - vector).magnitude;
				GameObject gameObject = wirePrefab.Instantiate(base.transform);
				gameObject.name = "WIRE";
				gameObject.transform.position = Vector3.Lerp(a, vector, 0.5f);
				gameObject.transform.LookAt(vector);
				gameObject.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(a, vector));
				gameObject.SetActive(true);
			}
		}
	}
}
