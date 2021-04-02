using UnityEngine;

public class EffectMount : EntityComponent<BaseEntity>, IClientComponent
{
	public bool firstPerson;

	public GameObject effectPrefab;

	public GameObject spawnedEffect;

	public GameObject mountBone;
}
