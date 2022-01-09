using UnityEngine;

public class PatternFireworkStar : MonoBehaviour, IClientComponent
{
	public GameObject Pixel;

	public GameObject Explosion;

	public ParticleSystem[] ParticleSystems;

	public void Initialize(Color color)
	{
		if (Pixel != null)
		{
			Pixel.SetActive(true);
		}
		if (Explosion != null)
		{
			Explosion.SetActive(false);
		}
		if (ParticleSystems == null)
		{
			return;
		}
		ParticleSystem[] particleSystems = ParticleSystems;
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (!(particleSystem == null))
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(color);
			}
		}
	}

	public void Explode()
	{
		if (Pixel != null)
		{
			Pixel.SetActive(false);
		}
		if (Explosion != null)
		{
			Explosion.SetActive(true);
		}
	}
}
