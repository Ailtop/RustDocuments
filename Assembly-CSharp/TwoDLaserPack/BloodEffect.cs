using UnityEngine;

namespace TwoDLaserPack
{
	public class BloodEffect : MonoBehaviour
	{
		public float fadespeed = 2f;

		public float timeBeforeFadeStarts = 1f;

		private float elapsedTimeBeforeFadeStarts;

		private SpriteRenderer sprite;

		private Color spriteColor;

		private void Awake()
		{
			sprite = base.gameObject.GetComponent<SpriteRenderer>();
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
			spriteColor = new Color(sprite.GetComponent<Renderer>().material.color.r, sprite.GetComponent<Renderer>().material.color.g, sprite.GetComponent<Renderer>().material.color.b, 1f);
		}

		private void Start()
		{
		}

		private void Update()
		{
			elapsedTimeBeforeFadeStarts += Time.deltaTime;
			if (elapsedTimeBeforeFadeStarts >= timeBeforeFadeStarts)
			{
				spriteColor = new Color(sprite.GetComponent<Renderer>().material.color.r, sprite.GetComponent<Renderer>().material.color.g, sprite.GetComponent<Renderer>().material.color.b, Mathf.Lerp(sprite.GetComponent<Renderer>().material.color.a, 0f, Time.deltaTime * fadespeed));
				sprite.GetComponent<Renderer>().material.color = spriteColor;
				if (sprite.material.color.a <= 0f)
				{
					base.gameObject.SetActive(false);
				}
			}
		}
	}
}
