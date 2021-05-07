using System.Collections;
using FX;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UserInput;

namespace UI
{
	public class NpcConversationBody : Dialogue
	{
		private const float _lettersPerSecond = 25f;

		private const float _intervalPerLetter = 0.04f;

		[SerializeField]
		private TextMeshProUGUI _textMeshPro;

		[SerializeField]
		private float _typeSpeed = 1f;

		[SerializeField]
		private SoundInfo _typeSoundInfo;

		public bool skippable { get; set; }

		public bool typing { get; private set; }

		public string text
		{
			get
			{
				return _textMeshPro.text;
			}
			set
			{
				_textMeshPro.text = value;
			}
		}

		public override bool closeWithPauseKey => false;

		protected override void OnEnable()
		{
			Dialogue.opened.Add(this);
		}

		protected override void OnDisable()
		{
			Dialogue.opened.Remove(this);
		}

		public IEnumerator CType(NpcConversation conversation)
		{
			typing = true;
			((TMP_Text)_textMeshPro).ForceMeshUpdate(false, false);
			int visibleCharacterCount = _textMeshPro.textInfo.characterCount;
			_textMeshPro.maxVisibleCharacters = 0;
			float interval = 0.04f * (1f / _typeSpeed);
			for (int index = 0; index < visibleCharacterCount; index++)
			{
				if (!typing)
				{
					yield break;
				}
				if (!conversation.visible)
				{
					break;
				}
				if (_textMeshPro.text[index] != ' ')
				{
					PersistentSingleton<SoundManager>.Instance.PlaySound(_typeSoundInfo, Singleton<Service>.Instance.levelManager.player.transform.position);
				}
				_textMeshPro.maxVisibleCharacters++;
				_textMeshPro.havePropertiesChanged = false;
				float time = 0f;
				while (time < interval)
				{
					if (!typing)
					{
						yield break;
					}
					yield return null;
					time += Time.unscaledDeltaTime;
					if (skippable && (KeyMapper.Map.Attack.WasPressed || KeyMapper.Map.Jump.WasPressed || KeyMapper.Map.Submit.WasPressed || KeyMapper.Map.Cancel.WasPressed))
					{
						goto end_IL_01a7;
					}
				}
				continue;
				end_IL_01a7:
				break;
			}
			_textMeshPro.maxVisibleCharacters = visibleCharacterCount;
			typing = false;
		}

		public void StopType()
		{
			_textMeshPro.maxVisibleCharacters = 0;
			_textMeshPro.havePropertiesChanged = false;
			typing = false;
		}
	}
}
