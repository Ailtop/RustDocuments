public interface IServerFileReceiver
{
	void OnServerFileReceived(FileStorage.Type type, uint numId, uint crc, byte[] data);
}
