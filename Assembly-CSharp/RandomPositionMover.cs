using UnityEngine;

public class RandomPositionMover : MonoBehaviour
{
	public float pickerInterval;

	public float radius;

	public GameObject player;

	public Vector2 randomPointInCircle;

	private void Start()
	{
		if (pickerInterval == 0f)
		{
			pickerInterval = 3f;
		}
		randomPointInCircle = Vector2.zero;
		InvokeRepeating("PickRandomPointInCircle", Random.Range(0f, pickerInterval), pickerInterval);
	}

	private void PickRandomPointInCircle()
	{
		base.transform.position = player.transform.position;
		randomPointInCircle = (Vector2)base.transform.localPosition + Random.insideUnitCircle * radius;
		base.transform.localPosition = randomPointInCircle;
	}

	private void Update()
	{
	}
}
