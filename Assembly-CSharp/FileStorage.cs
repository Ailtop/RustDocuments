#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Sqlite;
using Ionic.Crc;
using ProtoBuf;
using UnityEngine.Assertions;

public class FileStorage : IDisposable
{
	private class CacheData
	{
		public byte[] data;

		public NetworkableId entityID;

		public uint numID;
	}

	public enum Type
	{
		png = 0,
		jpg = 1,
		ogg = 2
	}

	private class FileDatabase : Facepunch.Sqlite.Database
	{
		public IEnumerable<AssociatedFiles.AssociatedFile> QueryAll(NetworkableId entityID)
		{
			IntPtr stmHandle = Prepare("SELECT filetype, crc, part, data FROM data WHERE entid = ?");
			Facepunch.Sqlite.Database.Bind(stmHandle, 1, entityID.Value);
			return ExecuteAndReadQueryResults(stmHandle, ReadAssociatedFileRow);
		}

		private static AssociatedFiles.AssociatedFile ReadAssociatedFileRow(IntPtr stmHandle)
		{
			AssociatedFiles.AssociatedFile associatedFile = Facepunch.Pool.Get<AssociatedFiles.AssociatedFile>();
			associatedFile.type = Facepunch.Sqlite.Database.GetColumnValue<int>(stmHandle, 0);
			associatedFile.crc = (uint)Facepunch.Sqlite.Database.GetColumnValue<int>(stmHandle, 1);
			associatedFile.numID = (uint)Facepunch.Sqlite.Database.GetColumnValue<int>(stmHandle, 2);
			associatedFile.data = Facepunch.Sqlite.Database.GetColumnValue<byte[]>(stmHandle, 3);
			return associatedFile;
		}
	}

	private FileDatabase db;

	private CRC32 crc = new CRC32();

	private MruDictionary<uint, CacheData> _cache = new MruDictionary<uint, CacheData>(1000);

	public static FileStorage server = new FileStorage("sv.files." + 243, server: true);

	protected FileStorage(string name, bool server)
	{
		if (server)
		{
			string path = Server.rootFolder + "/" + name + ".db";
			db = new FileDatabase();
			db.Open(path, fastMode: true);
			if (!db.TableExists("data"))
			{
				db.Execute("CREATE TABLE data ( crc INTEGER PRIMARY KEY, data BLOB, updated INTEGER, entid INTEGER, filetype INTEGER, part INTEGER )");
				db.Execute("CREATE INDEX IF NOT EXISTS entindex ON data ( entid )");
			}
		}
	}

	~FileStorage()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (db != null)
		{
			db.Close();
			db = null;
		}
	}

	private uint GetCRC(byte[] data, Type type)
	{
		using (TimeWarning.New("FileStorage.GetCRC"))
		{
			crc.Reset();
			crc.SlurpBlock(data, 0, data.Length);
			crc.UpdateCRC((byte)type);
			return (uint)crc.Crc32Result;
		}
	}

	public uint Store(byte[] data, Type type, NetworkableId entityID, uint numID = 0u)
	{
		using (TimeWarning.New("FileStorage.Store"))
		{
			uint cRC = GetCRC(data, type);
			if (db != null)
			{
				db.Execute("INSERT OR REPLACE INTO data ( crc, data, entid, filetype, part ) VALUES ( ?, ?, ?, ?, ? )", (int)cRC, data, (long)entityID.Value, (int)type, (int)numID);
			}
			_cache.Remove(cRC);
			_cache.Add(cRC, new CacheData
			{
				data = data,
				entityID = entityID,
				numID = numID
			});
			return cRC;
		}
	}

	public byte[] Get(uint crc, Type type, NetworkableId entityID, uint numID = 0u)
	{
		using (TimeWarning.New("FileStorage.Get"))
		{
			if (_cache.TryGetValue(crc, out var value))
			{
				Assert.IsTrue(value.data != null, "FileStorage cache contains a null texture");
				return value.data;
			}
			if (db == null)
			{
				return null;
			}
			byte[] array = db.Query<byte[], int, int, int, int>("SELECT data FROM data WHERE crc = ? AND filetype = ? AND entid = ? AND part = ? LIMIT 1", (int)crc, (int)type, (int)entityID.Value, (int)numID);
			if (array == null)
			{
				return null;
			}
			_cache.Remove(crc);
			_cache.Add(crc, new CacheData
			{
				data = array,
				entityID = entityID,
				numID = 0u
			});
			return array;
		}
	}

	public void Remove(uint crc, Type type, NetworkableId entityID)
	{
		using (TimeWarning.New("FileStorage.Remove"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE crc = ? AND filetype = ? AND entid = ?", (int)crc, (int)type, (long)entityID.Value);
			}
			_cache.Remove(crc);
		}
	}

	public void RemoveExact(uint crc, Type type, NetworkableId entityID, uint numid)
	{
		using (TimeWarning.New("FileStorage.RemoveExact"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE crc = ? AND filetype = ? AND entid = ? AND part = ?", (int)crc, (int)type, (long)entityID.Value, (int)numid);
			}
			_cache.Remove(crc);
		}
	}

	public void RemoveEntityNum(NetworkableId entityid, uint numid)
	{
		using (TimeWarning.New("FileStorage.RemoveEntityNum"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE entid = ? AND part = ?", (long)entityid.Value, (int)numid);
			}
			uint[] array = (from x in _cache
				where x.Value.entityID == entityid && x.Value.numID == numid
				select x.Key).ToArray();
			foreach (uint key in array)
			{
				_cache.Remove(key);
			}
		}
	}

	public void RemoveAllByEntity(NetworkableId entityid)
	{
		using (TimeWarning.New("FileStorage.RemoveAllByEntity"))
		{
			if (db != null)
			{
				db.Execute("DELETE FROM data WHERE entid = ?", (long)entityid.Value);
			}
		}
	}

	public void ReassignEntityId(NetworkableId oldId, NetworkableId newId)
	{
		using (TimeWarning.New("FileStorage.ReassignEntityId"))
		{
			if (db != null)
			{
				db.Execute("UPDATE data SET entid = ? WHERE entid = ?", (long)newId.Value, (long)oldId.Value);
			}
		}
	}

	public IEnumerable<AssociatedFiles.AssociatedFile> QueryAllByEntity(NetworkableId entityID)
	{
		return db.QueryAll(entityID);
	}
}
