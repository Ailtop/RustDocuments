using UnityEngine;

namespace TwoDLaserPack
{
	public class DemoToggleLaserActive : MonoBehaviour
	{
		public LineBasedLaser lineLaserRef;

		public SpriteBasedLaser spriteLaserRef;

		private void Start()
		{
		}

		private void OnMouseOver()
		{
			if (lineLaserRef != null && Input.GetMouseButtonDown(0))
			{
				lineLaserRef.SetLaserState(!lineLaserRef.laserActive);
			}
			if (spriteLaserRef != null && Input.GetMouseButtonDown(0))
			{
				spriteLaserRef.SetLaserState(!spriteLaserRef.laserActive);
			}
		}

		private void Update()
		{
		}
	}
}
