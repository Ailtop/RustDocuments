public class Client : SingletonComponent<Client>
{
	public static Translate.Phrase loading_loading = new Translate.Phrase("loading.loading", "Loading");

	public static Translate.Phrase loading_connecting = new Translate.Phrase("loading.connecting", "Connecting");

	public static Translate.Phrase loading_connectionaccepted = new Translate.Phrase("loading.connectionaccepted", "Connection Accepted");

	public static Translate.Phrase loading_connecting_negotiate = new Translate.Phrase("loading.connecting.negotiate", "Negotiating Connection");

	public static Translate.Phrase loading_level = new Translate.Phrase("loading.loadinglevel", "Loading Level");

	public static Translate.Phrase loading_skinnablewarmup = new Translate.Phrase("loading.skinnablewarmup", "Skinnable Warmup");

	public static Translate.Phrase loading_preloadcomplete = new Translate.Phrase("loading.preloadcomplete", "Preload Complete");

	public static Translate.Phrase loading_openingscene = new Translate.Phrase("loading.openingscene", "Opening Scene");

	public static Translate.Phrase loading_clientready = new Translate.Phrase("loading.clientready", "Client Ready");

	public static Translate.Phrase loading_prefabwarmup = new Translate.Phrase("loading.prefabwarmup", "Warming Prefabs [{0}/{1}]");
}
