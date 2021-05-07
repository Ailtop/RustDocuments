using Characters;
using Services;
using Singletons;
using UnityEngine;

public class CloseToPlayer : MonoBehaviour
{
	[SerializeField]
	private Character _owner;

	[SerializeField]
	private float _speed;

	private Transform _player;

	private void Start()
	{
		_player = Singleton<Service>.Instance.levelManager.player.transform;
	}

	private void Update()
	{
		float num = Mathf.Sign(_player.position.x - base.transform.position.x);
		base.transform.Translate(new Vector2(num * _owner.chronometer.master.deltaTime * _speed, 0f));
	}
}
