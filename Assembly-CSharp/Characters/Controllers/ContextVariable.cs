namespace Characters.Controllers
{
	public struct ContextVariable<T>
	{
		public readonly Context context;

		public readonly string name;

		public T value
		{
			get
			{
				return context.Get<T>(name);
			}
			set
			{
				context.Set(name, value);
			}
		}

		public ContextVariable(Context context, string name)
		{
			this.context = context;
			this.name = name;
		}
	}
	public struct ContextVariable<TKey, TVal>
	{
		public readonly Context<TKey> context;

		public readonly TKey key;

		public TVal value
		{
			get
			{
				return context.Get<TVal>(key);
			}
			set
			{
				context.Set(key, value);
			}
		}

		public ContextVariable(Context<TKey> context, TKey key)
		{
			this.context = context;
			this.key = key;
		}
	}
}
