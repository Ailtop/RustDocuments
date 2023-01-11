using UnityEngine;

public class JiggleBone : BaseMonoBehaviour
{
	public bool debugMode = true;

	private Vector3 targetPos;

	private Vector3 dynamicPos;

	public Vector3 boneAxis = new Vector3(0f, 0f, 1f);

	public float targetDistance = 2f;

	public float bStiffness = 0.1f;

	public float bMass = 0.9f;

	public float bDamping = 0.75f;

	public float bGravity = 0.75f;

	private Vector3 force;

	private Vector3 acc;

	private Vector3 vel;

	public bool SquashAndStretch = true;

	public float sideStretch = 0.15f;

	public float frontStretch = 0.2f;

	public float disableDistance = 20f;

	private void Awake()
	{
		Vector3 vector = base.transform.position + base.transform.TransformDirection(new Vector3(boneAxis.x * targetDistance, boneAxis.y * targetDistance, boneAxis.z * targetDistance));
		dynamicPos = vector;
	}

	private void LateUpdate()
	{
		base.transform.rotation = default(Quaternion);
		Vector3 dir = base.transform.TransformDirection(new Vector3(boneAxis.x * targetDistance, boneAxis.y * targetDistance, boneAxis.z * targetDistance));
		Vector3 vector = base.transform.TransformDirection(new Vector3(0f, 1f, 0f));
		Vector3 vector2 = base.transform.position + base.transform.TransformDirection(new Vector3(boneAxis.x * targetDistance, boneAxis.y * targetDistance, boneAxis.z * targetDistance));
		force.x = (vector2.x - dynamicPos.x) * bStiffness;
		acc.x = force.x / bMass;
		vel.x += acc.x * (1f - bDamping);
		force.y = (vector2.y - dynamicPos.y) * bStiffness;
		force.y -= bGravity / 10f;
		acc.y = force.y / bMass;
		vel.y += acc.y * (1f - bDamping);
		force.z = (vector2.z - dynamicPos.z) * bStiffness;
		acc.z = force.z / bMass;
		vel.z += acc.z * (1f - bDamping);
		dynamicPos += vel + force;
		base.transform.LookAt(dynamicPos, vector);
		if (SquashAndStretch)
		{
			float magnitude = (dynamicPos - vector2).magnitude;
			float x = ((boneAxis.x != 0f) ? (1f + magnitude * frontStretch) : (1f + (0f - magnitude) * sideStretch));
			float y = ((boneAxis.y != 0f) ? (1f + magnitude * frontStretch) : (1f + (0f - magnitude) * sideStretch));
			float z = ((boneAxis.z != 0f) ? (1f + magnitude * frontStretch) : (1f + (0f - magnitude) * sideStretch));
			base.transform.localScale = new Vector3(x, y, z);
		}
		if (debugMode)
		{
			Debug.DrawRay(base.transform.position, dir, Color.blue);
			Debug.DrawRay(base.transform.position, vector, Color.green);
			Debug.DrawRay(vector2, Vector3.up * 0.2f, Color.yellow);
			Debug.DrawRay(dynamicPos, Vector3.up * 0.2f, Color.red);
		}
	}
}
