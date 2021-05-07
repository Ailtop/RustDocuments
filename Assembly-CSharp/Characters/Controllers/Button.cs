using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Characters.Controllers
{
	public sealed class Button
	{
		public class StringPopupAttribute : PopupAttribute
		{
			public StringPopupAttribute()
				: base(false, _names)
			{
			}
		}

		public static readonly Button Attack;

		public static readonly Button Dash;

		public static readonly Button Jump;

		public static readonly Button Skill;

		public static readonly Button Skill2;

		public static readonly Button UseItem;

		public static readonly Button None;

		public static readonly int count;

		public static readonly ReadOnlyCollection<Button> values;

		public static readonly ReadOnlyCollection<string> names;

		private static readonly string[] _names;

		private static Button[] _values;

		private static int _count;

		public readonly string name;

		public readonly int index;

		static Button()
		{
			_values = new Button[7]
			{
				Attack = new Button("Attack"),
				Dash = new Button("Dash"),
				Jump = new Button("Jump"),
				Skill = new Button("Skill"),
				Skill2 = new Button("Skill2"),
				UseItem = new Button("UseItem"),
				None = new Button("None")
			};
			_names = _values.Select((Button kind) => kind.name).ToArray();
			values = Array.AsReadOnly(_values);
			names = Array.AsReadOnly(_names);
			count = _count;
		}

		private Button(string name)
		{
			this.name = name;
			index = _count++;
		}

		public override string ToString()
		{
			return name;
		}
	}
}
