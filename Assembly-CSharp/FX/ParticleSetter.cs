using UnityEngine;

namespace FX
{
	public class ParticleSetter : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem _effect;

		[SerializeField]
		private BoxCollider2D _range;

		[SerializeField]
		private float _emmitPerTile;

		private void Awake()
		{
			Object.Destroy(this);
		}
	}
}
