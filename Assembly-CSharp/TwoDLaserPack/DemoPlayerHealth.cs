using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TwoDLaserPack
{
	public class DemoPlayerHealth : MonoBehaviour
	{
		public GameObject bloodSplatPrefab;

		public GameObject playerPrefab;

		public Button restartButton;

		public Text healthText;

		private LineBasedLaser[] allLasersInScene;

		public ParticleSystem bloodParticleSystem;

		[SerializeField]
		private int _healthPoints;

		public int HealthPoints
		{
			get
			{
				return _healthPoints;
			}
			set
			{
				_healthPoints = value;
				if (_healthPoints <= 0)
				{
					if (bloodSplatPrefab != null)
					{
						Object.Instantiate(bloodSplatPrefab, base.transform.position, Quaternion.identity);
					}
					healthText.text = "Health: 0";
					base.gameObject.GetComponent<Renderer>().enabled = false;
					base.gameObject.GetComponent<PlayerMovement>().enabled = false;
					restartButton.gameObject.SetActive(true);
					LineBasedLaser[] array = allLasersInScene;
					foreach (LineBasedLaser obj in array)
					{
						obj.OnLaserHitTriggered -= LaserOnOnLaserHitTriggered;
						obj.SetLaserState(false);
					}
				}
				else
				{
					healthText.text = "Health: " + _healthPoints;
				}
			}
		}

		private void Start()
		{
			_healthPoints = 10;
			if (restartButton == null)
			{
				restartButton = Object.FindObjectsOfType<Button>().FirstOrDefault((Button b) => b.name == "ButtonReplay");
			}
			healthText = Object.FindObjectsOfType<Text>().FirstOrDefault((Text t) => t.name == "TextHealth");
			healthText.text = "Health: 10";
			allLasersInScene = Object.FindObjectsOfType<LineBasedLaser>();
			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(OnRestartButtonClick);
			if (allLasersInScene.Any())
			{
				LineBasedLaser[] array = allLasersInScene;
				foreach (LineBasedLaser obj in array)
				{
					obj.OnLaserHitTriggered += LaserOnOnLaserHitTriggered;
					obj.SetLaserState(true);
					obj.targetGo = base.gameObject;
				}
			}
			base.gameObject.GetComponent<PlayerMovement>().enabled = true;
			base.gameObject.GetComponent<Renderer>().enabled = true;
			restartButton.gameObject.SetActive(false);
		}

		private void OnRestartButtonClick()
		{
			CreateNewPlayer();
			Object.Destroy(base.gameObject);
		}

		private void CreateNewPlayer()
		{
			GameObject targetGo = Object.Instantiate(playerPrefab, new Vector2(6.26f, -2.8f), Quaternion.identity);
			LineBasedLaser[] array = allLasersInScene;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].targetGo = targetGo;
			}
		}

		private void LaserOnOnLaserHitTriggered(RaycastHit2D hitInfo)
		{
			if (hitInfo.collider.gameObject == base.gameObject && bloodParticleSystem != null)
			{
				bloodParticleSystem.Play();
				HealthPoints--;
			}
		}

		private void Update()
		{
		}
	}
}
