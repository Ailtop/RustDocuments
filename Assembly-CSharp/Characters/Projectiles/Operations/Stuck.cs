using System.Collections;
using FX;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class Stuck : HitOperation
	{
		[Information("0일 경우 삭제 되지 않음", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _lifeTime;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Sprite _spriteToReplace;

		[SerializeField]
		private EffectInfo _despawnEffect;

		[SerializeField]
		private Vector2 _despawnEffectSpawnOffset;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			if (_spriteRenderer == null)
			{
				_spriteRenderer = projectile.GetComponentInParent<SpriteRenderer>();
			}
			if (!(_spriteRenderer == null))
			{
				Effects.SpritePoolObject effect = Effects.sprite.Spawn();
				effect.spriteRenderer.CopyFrom(_spriteRenderer);
				if (_spriteToReplace != null)
				{
					effect.spriteRenderer.sprite = _spriteToReplace;
				}
				effect.spriteRenderer.sortingOrder--;
				effect.spriteRenderer.color = _spriteRenderer.color;
				Transform obj = effect.poolObject.transform;
				obj.position = raycastHit.point;
				obj.localScale = _spriteRenderer.transform.lossyScale;
				obj.rotation = _spriteRenderer.transform.rotation;
				effect.poolObject.StartCoroutine(Despawn(effect));
			}
		}

		private IEnumerator Despawn(Effects.SpritePoolObject effect)
		{
			yield return Chronometer.global.WaitForSeconds(_lifeTime);
			if (_despawnEffect != null)
			{
				Vector2 vector = new Vector2(effect.poolObject.transform.position.x + _despawnEffectSpawnOffset.x, effect.poolObject.transform.position.y + _despawnEffectSpawnOffset.y);
				_despawnEffect.Spawn(vector);
			}
			effect.poolObject.Despawn();
		}
	}
}
