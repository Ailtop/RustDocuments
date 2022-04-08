using ConVar;
using UnityEngine;

namespace Rust.Ai;

public class ScientistSpawner : SpawnGroup
{
	[Header("Scientist Spawner")]
	public bool Mobile = true;

	public bool NeverMove;

	public bool SpawnHostile;

	public bool OnlyAggroMarkedTargets = true;

	public bool IsPeacekeeper = true;

	public bool IsBandit;

	public bool IsMilitaryTunnelLab;

	public WaypointSet Waypoints;

	public Transform[] LookAtInterestPointsStationary;

	public Vector2 RadioEffectRepeatRange = new Vector2(10f, 15f);

	public Model Model;

	[SerializeField]
	private AiLocationManager _mgr;

	private float _nextForcedRespawn = float.PositiveInfinity;

	private bool _lastSpawnCallHadAliveMembers;

	private bool _lastSpawnCallHadMaxAliveMembers;

	protected override void Spawn(int numToSpawn)
	{
		if (!ConVar.AI.npc_enable)
		{
			return;
		}
		if (base.currentPopulation == maxPopulation)
		{
			_lastSpawnCallHadMaxAliveMembers = true;
			_lastSpawnCallHadAliveMembers = true;
			return;
		}
		if (_lastSpawnCallHadMaxAliveMembers)
		{
			_nextForcedRespawn = UnityEngine.Time.time + 2200f;
		}
		if (UnityEngine.Time.time < _nextForcedRespawn)
		{
			if (base.currentPopulation == 0 && _lastSpawnCallHadAliveMembers)
			{
				_lastSpawnCallHadMaxAliveMembers = false;
				_lastSpawnCallHadAliveMembers = false;
				return;
			}
			if (base.currentPopulation > 0)
			{
				_lastSpawnCallHadMaxAliveMembers = false;
				_lastSpawnCallHadAliveMembers = base.currentPopulation > 0;
				return;
			}
		}
		_lastSpawnCallHadMaxAliveMembers = false;
		_lastSpawnCallHadAliveMembers = base.currentPopulation > 0;
		base.Spawn(numToSpawn);
	}

	protected override void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (LookAtInterestPointsStationary == null || LookAtInterestPointsStationary.Length == 0)
		{
			return;
		}
		Gizmos.color = Color.magenta - new Color(0f, 0f, 0f, 0.5f);
		Transform[] lookAtInterestPointsStationary = LookAtInterestPointsStationary;
		foreach (Transform transform in lookAtInterestPointsStationary)
		{
			if (transform != null)
			{
				Gizmos.DrawSphere(transform.position, 0.1f);
				Gizmos.DrawLine(base.transform.position, transform.position);
			}
		}
	}
}
