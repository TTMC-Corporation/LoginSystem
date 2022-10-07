using Auram;

namespace TTMC.LoginSystem
{
	public class Account
	{
		private static Database accounts = new Database();
		private static void LoadConfig()
		{
			if (accounts.data.Count <= 0)
			{
				string path = "Base" + Path.DirectorySeparatorChar + "accounts.auram";
				if (File.Exists(path))
				{
					accounts.Load(path);
				}
			}
		}
		private static void SaveConfig()
		{
			string path = "Base" + Path.DirectorySeparatorChar + "accounts.auram";
			Directory.CreateDirectory("Base");
			accounts.Save(path);
		}
		public static Token? Login(string username, string password)
		{
			LoadConfig();
			byte[]? raw = accounts.Get(username);
			if (raw != null)
			{
				Guid guid = new Guid(raw);
				string path = "Account" + Path.DirectorySeparatorChar + guid;
				if (File.Exists(path))
				{
					try
					{
						byte[] nzx = Encryption.Decrypt(File.ReadAllBytes(path), password);
						if (new Guid(nzx) == guid)
						{
							return Token.Generate(guid);
						}
					}
					catch
					{
						return null;
					}
				}
			}
			return null;
		}
		public static Token? Refresh(byte[] accessToken)
		{
			LoadConfig();
			return Token.Refresh(accessToken);
		}
		public static bool Register(string username, string password)
		{
			LoadConfig();
			if (!accounts.data.ContainsKey(username))
			{
				Directory.CreateDirectory("Account");
				Guid guid = Guid.NewGuid();
				string path = "Account" + Path.DirectorySeparatorChar + guid;
				File.WriteAllBytes(path, Encryption.Encrypt(guid.ToByteArray(), password));
				accounts.Set(username, guid.ToByteArray());
				SaveConfig();
				return true;
			}
			return false;
		}
		public static Guid? Auth(byte[] accessToken)
		{
			LoadConfig();
			return Token.Owner(accessToken);
		}
	}
}