using System;
using System.Runtime.InteropServices;
using Rust.Instruments;
using UnityEngine;

public class InstrumentKeyController : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NoteBinding
	{
	}

	public enum IKType
	{
		LeftHand = 0,
		RightHand = 1,
		RightFoot = 2
	}

	public enum NoteType
	{
		Regular = 0,
		Sharp = 1
	}

	public enum InstrumentType
	{
		Note = 0,
		Hold = 1
	}

	public enum AnimationSlot
	{
		None = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7
	}

	[Serializable]
	public struct KeySet
	{
		public Notes Note;

		public NoteType NoteType;

		public int OctaveShift;

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", Note, (NoteType == NoteType.Sharp) ? "#" : string.Empty, OctaveShift);
		}
	}

	public struct NoteOverride
	{
		public bool Override;

		public KeySet Note;
	}

	[Serializable]
	public struct IKNoteTarget
	{
		public IKType TargetType;

		public int IkIndex;
	}

	public const float DEFAULT_NOTE_VELOCITY = 1f;

	public NoteBindingCollection Bindings;

	public NoteBinding[] NoteBindings = new NoteBinding[0];

	public Transform[] NoteSoundPositions;

	public InstrumentIKController IKController;

	public Transform LeftHandProp;

	public Transform RightHandProp;

	public Animator InstrumentAnimator;

	public BaseEntity RPCHandler;

	public uint overrideAchievementId;

	private const string ALL_NOTES_STATNAME = "played_notes";

	public bool PlayedNoteThisFrame { get; private set; }

	public void ProcessServerPlayedNote(BasePlayer forPlayer)
	{
		if (!(forPlayer == null))
		{
			forPlayer.stats.Add(Bindings.NotePlayedStatName, 1, (Stats)5);
			forPlayer.stats.Add("played_notes", 1, (Stats)5);
		}
	}
}
