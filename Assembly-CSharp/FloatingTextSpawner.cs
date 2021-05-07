using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using Scenes;
using UI;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
	private const int _countLimit = 100;

	private const string _buffDefaultColorString = "#F2F2F2";

	public FloatingText floatingTextPrefab;

	private const int _buffTextLimit = 10;

	public BuffText _buffTextPrefab;

	private Vector3 _playerTakingDamageScale = new Vector3(1.25f, 1.25f, 1f);

	private Color _playerTakingDamageColor;

	private Color _criticalPhysicalAttackColor;

	private Color _criticalMagicAttackColor;

	private Color _physicalAttackColor;

	private Color _magicAttackColor;

	private Color _fixedAttackColor;

	private Color _healColor;

	private void Awake()
	{
		ColorUtility.TryParseHtmlString("#FF0D06", out _playerTakingDamageColor);
		ColorUtility.TryParseHtmlString("#E3FF00", out _criticalPhysicalAttackColor);
		ColorUtility.TryParseHtmlString("#17FFDB", out _criticalMagicAttackColor);
		ColorUtility.TryParseHtmlString("#FF8000", out _physicalAttackColor);
		ColorUtility.TryParseHtmlString("#17C4FF", out _magicAttackColor);
		ColorUtility.TryParseHtmlString("#F2F2F2", out _fixedAttackColor);
		ColorUtility.TryParseHtmlString("#18FF00", out _healColor);
		StartCoroutine(floatingTextPrefab.poolObject.CPreloadAsync(100));
		floatingTextPrefab.poolObject.Limit(100);
		StartCoroutine(floatingTextPrefab.poolObject.CPreloadAsync(10));
		floatingTextPrefab.poolObject.Limit(10);
	}

	public FloatingText Spawn(string text, Vector3 position)
	{
		FloatingText floatingText = floatingTextPrefab.Spawn();
		floatingText.Initialize(text, position);
		return floatingText;
	}

	public void SpawnPlayerTakingDamage([In][IsReadOnly] ref Damage damage)
	{
		Damage damage2 = damage;
		SpawnPlayerTakingDamage(damage2.amount, damage.hitPoint);
	}

	public void SpawnPlayerTakingDamage(double amount, Vector2 position)
	{
		if (Scene<GameBase>.instance.uiManager.hideOption != UIManager.HideOption.HideAll && !(amount < 1.0))
		{
			FloatingText floatingText = Spawn(amount.ToString("0"), position + new Vector2(0f, 0.5f));
			floatingText.color = _playerTakingDamageColor;
			floatingText.Modify(GameObjectModifier.TranslateBySpeedAndAcc(9f, -12f, 2.5f));
			floatingText.sortingOrder = 500;
			floatingText.transform.localScale = _playerTakingDamageScale;
			if (MMMaths.RandomBool())
			{
				floatingText.Modify(GameObjectModifier.TranslateUniformMotion(0.2f, 0f, 0f));
			}
			else
			{
				floatingText.Modify(GameObjectModifier.TranslateUniformMotion(-0.2f, 0f, 0f));
			}
			floatingText.Despawn(0.6f);
		}
	}

	public void SpawnTakingDamage([In][IsReadOnly] ref Damage damage)
	{
		if (Scene<GameBase>.instance.uiManager.hideOption == UIManager.HideOption.HideAll)
		{
			return;
		}
		Damage damage2 = damage;
		if (damage2.amount < 1.0)
		{
			return;
		}
		damage2 = damage;
		FloatingText floatingText = Spawn(damage2.ToString(), damage.hitPoint + new Vector2(0f, 0.5f));
		switch (damage.attribute)
		{
		case Damage.Attribute.Physical:
			floatingText.color = _physicalAttackColor;
			floatingText.sortingOrder = 100;
			break;
		case Damage.Attribute.Magic:
			floatingText.color = _magicAttackColor;
			floatingText.sortingOrder = 200;
			break;
		case Damage.Attribute.Fixed:
			floatingText.color = _fixedAttackColor;
			floatingText.sortingOrder = 100;
			break;
		}
		if (damage.critical)
		{
			floatingText.Modify(GameObjectModifier.LerpScale(1.4f, 1.6f, 0.4f));
			switch (damage.attribute)
			{
			case Damage.Attribute.Physical:
				floatingText.color = _criticalPhysicalAttackColor;
				floatingText.sortingOrder = 300;
				break;
			case Damage.Attribute.Magic:
				floatingText.color = _criticalMagicAttackColor;
				floatingText.sortingOrder = 400;
				break;
			}
		}
		floatingText.Modify(GameObjectModifier.TranslateBySpeedAndAcc(10f, -17f, 3f));
		if (MMMaths.RandomBool())
		{
			floatingText.Modify(GameObjectModifier.TranslateUniformMotion(0.2f, 0f, 0f));
		}
		else
		{
			floatingText.Modify(GameObjectModifier.TranslateUniformMotion(-0.2f, 0f, 0f));
		}
		floatingText.Despawn(0.5f);
	}

	public void SpawnHeal(double amount, Vector3 position)
	{
		if (Scene<GameBase>.instance.uiManager.hideOption != UIManager.HideOption.HideAll && !(amount <= 0.0))
		{
			FloatingText floatingText = Spawn(amount.ToString("0"), position);
			floatingText.color = _healColor;
			floatingText.Modify(GameObjectModifier.Scale(1.1f));
			floatingText.Modify(GameObjectModifier.TranslateBySpeedAndAcc(7.5f, -7.5f, 1f));
			floatingText.sortingOrder = 1;
			if (MMMaths.RandomBool())
			{
				floatingText.Modify(GameObjectModifier.TranslateUniformMotion(0.2f, 0f, 0f));
			}
			else
			{
				floatingText.Modify(GameObjectModifier.TranslateUniformMotion(-0.2f, 0f, 0f));
			}
			floatingText.Despawn(0.5f);
		}
	}

	public void SpawnBuff(string text, Vector3 position, string colorValue = "#F2F2F2")
	{
		if (Scene<GameBase>.instance.uiManager.hideOption != UIManager.HideOption.HideAll)
		{
			Color color;
			ColorUtility.TryParseHtmlString(colorValue, out color);
			BuffText buffText = _buffTextPrefab.Spawn();
			buffText.Initialize(text, position);
			buffText.color = color;
			buffText.sortingOrder = 1;
			StartCoroutine(CMove(buffText.transform, 0.5f, 2f));
			StartCoroutine(CFadeOut(buffText, 1.5f, 0.5f));
			buffText.Despawn(2f);
		}
	}

	public void SpawnStatus(string text, Vector3 position, string colorValue)
	{
		if (Scene<GameBase>.instance.uiManager.hideOption != UIManager.HideOption.HideAll)
		{
			Color color;
			ColorUtility.TryParseHtmlString(colorValue, out color);
			FloatingText floatingText = Spawn(text, position);
			floatingText.color = color;
			floatingText.sortingOrder = 1;
			floatingText.Modify(GameObjectModifier.Scale(0.75f));
			StartCoroutine(CMove(floatingText.transform, 0.5f, 1f));
			StartCoroutine(CFadeOut(floatingText, 0.5f, 0.5f));
			floatingText.Despawn(1f);
		}
	}

	public void SpawnEvade(string text, Vector3 position, string colorValue)
	{
		if (Scene<GameBase>.instance.uiManager.hideOption != UIManager.HideOption.HideAll)
		{
			Color color;
			ColorUtility.TryParseHtmlString(colorValue, out color);
			FloatingText floatingText = Spawn(text, position);
			floatingText.color = color;
			floatingText.sortingOrder = 1;
			floatingText.Modify(GameObjectModifier.Scale(1f));
			StartCoroutine(CMove(floatingText.transform, 0.5f, 1f));
			StartCoroutine(CFadeOut(floatingText, 0.5f, 0.5f));
			floatingText.Despawn(1f);
		}
	}

	private IEnumerator CMove(Transform transform, float distance, float duration)
	{
		float elapsed = 0f;
		float speed = distance / duration;
		while (elapsed <= duration)
		{
			float deltaTime = Chronometer.global.deltaTime;
			float num = speed * deltaTime;
			elapsed += deltaTime;
			transform.Translate(Vector2.up * num);
			yield return null;
		}
	}

	private IEnumerator CFadeOut(FloatingText spawned, float delay, float duration)
	{
		yield return Chronometer.global.WaitForSeconds(delay);
		spawned.FadeOut(duration);
	}

	private IEnumerator CFadeOut(BuffText spawned, float delay, float duration)
	{
		yield return Chronometer.global.WaitForSeconds(delay);
		spawned.FadeOut(duration);
	}
}
