using System.Collections.Generic;
using System.Linq;
using Characters.Actions;
using Characters.Cooldowns;
using UnityEngine;

public class SkulHeadController : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Action _action;

	[SerializeField]
	private Sprite[] _skulSprites;

	[SerializeField]
	private Sprite[] _skulHeadlessSprites;

	private Dictionary<string, Sprite> _skulSpritesMap;

	private Dictionary<string, Sprite> _skulHeadlessSpritesMap;

	public CooldownSerializer cooldown => _action.cooldown;

	private void Awake()
	{
		_skulSpritesMap = _skulSprites.ToDictionary((Sprite s) => s.name);
		_skulHeadlessSpritesMap = _skulHeadlessSprites.ToDictionary((Sprite s) => s.name);
	}

	private void LateUpdate()
	{
		Sprite value;
		if (_action.cooldown.canUse)
		{
			if (_skulSpritesMap.TryGetValue(_spriteRenderer.sprite.name, out value))
			{
				_spriteRenderer.sprite = value;
			}
		}
		else if (_skulHeadlessSpritesMap.TryGetValue(_spriteRenderer.sprite.name, out value))
		{
			_spriteRenderer.sprite = value;
		}
	}
}
