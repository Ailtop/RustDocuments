using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Characters
{
	public delegate void TookDamageDelegate([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt);
}
