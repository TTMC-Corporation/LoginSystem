using Auram;
using System.IO;
using System.Net.Security;
using System.Text;

namespace TTMC.LoginSystem
{
	public class Permission
	{
		public static bool CheckPermission(Guid guid, string permission)
		{
			string path = "Permission" + Path.DirectorySeparatorChar + guid + ".auram";
			if (File.Exists(path))
			{
				Database db = new Database(path);
				foreach (byte[] raw in db.data.Values)
				{
					if (Encoding.UTF8.GetString(raw) == permission)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static bool SetPermission(Guid guid, string permission)
		{
			List<string> permissions = new();
			string path = "Permission" + Path.DirectorySeparatorChar + guid + ".auram";
			Database db = new Database();
			if (File.Exists(path))
			{
				db.Load(path);
			}
			Directory.CreateDirectory("Permission");
			if (!CheckPermission(guid, permission))
			{
				db.Set(db.data.Keys.Count.ToString(), Encoding.UTF8.GetBytes(permission));
				db.Save(path);
				return true;
			}
			return false;
		}
	}
}