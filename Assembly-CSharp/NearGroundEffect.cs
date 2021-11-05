using UnityEngine;

public class NearGroundEffect : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private ParticleSystem ps;

	[SerializeField]
	private float nearGroundMetres = 10f;
}
