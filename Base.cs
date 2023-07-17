using TTMC.Auram;
using System.Security.Cryptography;
using System.Text;

namespace TTMC.LoginSystem
{
	public class Account
	{
		private static SHA512 sha = SHA512.Create();
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
			byte[]? raw = accounts.Get<byte[]>(username);
			if (raw != null)
			{
				Guid guid = new Guid(raw);
				string path = "Account" + Path.DirectorySeparatorChar + guid + ".auram";
				if (File.Exists(path))
				{
					Database user = new(path);
					byte[] hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
					byte[]? stored = user.Get<byte[]>("Password");
					if (stored != null && Convert.ToHexString(stored) == Convert.ToHexString(hashed))
					{
						return Token.Generate(guid);
					}
				}
			}
			return null;
		}
		public static Token? Refresh(byte[] refreshToken)
		{
			LoadConfig();
			return Token.Refresh(refreshToken);
		}
		public static bool Exists(string username)
		{
			LoadConfig();
			return accounts.data.ContainsKey(username);
		}
		public static Database? Register(string username, string password)
		{
			LoadConfig();
			if (!Exists(username))
			{
				Directory.CreateDirectory("Account");
				Guid guid = Guid.NewGuid();
				string path = "Account" + Path.DirectorySeparatorChar + guid + ".auram";
				while (File.Exists(path))
				{
					guid = Guid.NewGuid();
					path = "Account" + Path.DirectorySeparatorChar + guid + ".auram";
				}
				Database user = new();
				byte[] hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
				user.Set("Username", username);
				user.Set("Password", hashed);
				user.Save(path);
				accounts.Set(username, guid.ToByteArray());
				SaveConfig();
				return user;
			}
			return null;
		}
		public static Database? GetDatabase(Guid guid)
		{
			string path = "Account" + Path.DirectorySeparatorChar + guid + ".auram";
			return new(path);
		}
		public static Guid? Auth(byte[] accessToken)
		{
			LoadConfig();
			return Token.Owner(accessToken);
		}
	}
}