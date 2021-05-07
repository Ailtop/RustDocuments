using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Characters
{
	public delegate void GaveDamageDelegate(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt);
}
