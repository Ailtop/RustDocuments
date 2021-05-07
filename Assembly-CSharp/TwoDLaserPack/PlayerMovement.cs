using System;
using UnityEngine;

namespace TwoDLaserPack
{
	public class PlayerMovement : MonoBehaviour
	{
		public enum PlayerMovementType
		{
			Normal,
			FreeAim
		}

		public PlayerMovementType playerMovementType;

		public bool IsMoving;

		public float aimAngle;

		[Range(1f, 5f)]
		public float freeAimMovementSpeed = 2f;

		private float SmoothSpeedX;

		private float SmoothSpeedY;

		private const float SmoothMaxSpeedX = 7f;

		private const float SmoothMaxSpeedY = 7f;

		private const float AccelerationX = 22f;

		private const float AccelerationY = 22f;

		private const float DecelerationX = 33f;

		private const float DecelerationY = 33f;

		private Animator playerAnimator;

		private void Start()
		{
			if (base.gameObject.GetComponent<Animator>() != null)
			{
				playerAnimator = base.gameObject.GetComponent<Animator>();
			}
		}

		private void moveForward(float amount)
		{
			Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y + amount * Time.deltaTime, base.transform.position.z);
			base.transform.position = position;
		}

		private void moveBack(float amount)
		{
			Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - amount * Time.deltaTime, base.transform.position.z);
			base.transform.position = position;
		}

		private void moveRight(float amount)
		{
			Vector3 position = new Vector3(base.transform.position.x + amount * Time.deltaTime, base.transform.position.y, base.transform.position.z);
			base.transform.position = position;
		}

		private void moveLeft(float amount)
		{
			Vector3 position = new Vector3(base.transform.position.x - amount * Time.deltaTime, base.transform.position.y, base.transform.position.z);
			base.transform.position = position;
		}

		private void HandlePlayerToggles()
		{
		}

		private void HandlePlayerMovement()
		{
			float axis = Input.GetAxis("Horizontal");
			float axis2 = Input.GetAxis("Vertical");
			if (Mathf.Abs(axis) > 0f || Mathf.Abs(axis2) > 0f)
			{
				IsMoving = true;
				if (playerAnimator != null)
				{
					playerAnimator.SetBool("IsMoving", true);
				}
			}
			else
			{
				IsMoving = false;
				if (playerAnimator != null)
				{
					playerAnimator.SetBool("IsMoving", false);
				}
			}
			Vector2 facingDirection = Vector2.zero;
			switch (playerMovementType)
			{
			case PlayerMovementType.Normal:
			{
				if (axis < 0f && SmoothSpeedX > -7f)
				{
					SmoothSpeedX -= 22f * Time.deltaTime;
				}
				else if (axis > 0f && SmoothSpeedX < 7f)
				{
					SmoothSpeedX += 22f * Time.deltaTime;
				}
				else if (SmoothSpeedX > 33f * Time.deltaTime)
				{
					SmoothSpeedX -= 33f * Time.deltaTime;
				}
				else if (SmoothSpeedX < -33f * Time.deltaTime)
				{
					SmoothSpeedX += 33f * Time.deltaTime;
				}
				else
				{
					SmoothSpeedX = 0f;
				}
				if (axis2 < 0f && SmoothSpeedY > -7f)
				{
					SmoothSpeedY -= 22f * Time.deltaTime;
				}
				else if (axis2 > 0f && SmoothSpeedY < 7f)
				{
					SmoothSpeedY += 22f * Time.deltaTime;
				}
				else if (SmoothSpeedY > 33f * Time.deltaTime)
				{
					SmoothSpeedY -= 33f * Time.deltaTime;
				}
				else if (SmoothSpeedY < -33f * Time.deltaTime)
				{
					SmoothSpeedY += 33f * Time.deltaTime;
				}
				else
				{
					SmoothSpeedY = 0f;
				}
				Vector2 vector = new Vector2(base.transform.position.x + SmoothSpeedX * Time.deltaTime, base.transform.position.y + SmoothSpeedY * Time.deltaTime);
				base.transform.position = vector;
				break;
			}
			case PlayerMovementType.FreeAim:
				if (axis2 > 0f)
				{
					moveForward(freeAimMovementSpeed);
				}
				else if (axis2 < 0f)
				{
					moveBack(freeAimMovementSpeed);
				}
				if (axis > 0f)
				{
					moveRight(freeAimMovementSpeed);
				}
				else if (axis < 0f)
				{
					moveLeft(freeAimMovementSpeed);
				}
				facingDirection = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)) - base.transform.position;
				break;
			}
			CalculateAimAndFacingAngles(facingDirection);
			Vector3 position = Camera.main.WorldToViewportPoint(base.transform.position);
			position.x = Mathf.Clamp(position.x, 0.05f, 0.95f);
			position.y = Mathf.Clamp(position.y, 0.05f, 0.95f);
			base.transform.position = Camera.main.ViewportToWorldPoint(position);
		}

		private void CalculateAimAndFacingAngles(Vector2 facingDirection)
		{
			aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
			if (aimAngle < 0f)
			{
				aimAngle = (float)Math.PI * 2f + aimAngle;
			}
			base.transform.eulerAngles = new Vector3(0f, 0f, aimAngle * 57.29578f);
		}

		private void Update()
		{
			HandlePlayerMovement();
			HandlePlayerToggles();
		}
	}
}
