using Auram;

namespace TTMC.LoginSystem
{
	public class Token
	{
		private static bool loaded = false;
		private static List<Token> list = new List<Token>();
		public Guid owner = Guid.Empty;
		public byte[] accessToken = new byte[64];
		public byte[] refreshToken = new byte[32];
		public ushort expiresIn = 0;
		internal DateTime expire1 = new();
		internal DateTime expire2 = new();
		private static Random random = new();
		public Token(Guid guid)
		{
			owner = guid;
			random.NextBytes(accessToken);
			while (list.Any(x => x.accessToken == accessToken))
			{
				random.NextBytes(accessToken);
			}
			random.NextBytes(refreshToken);
			while (list.Any(x => x.refreshToken == refreshToken))
			{
				random.NextBytes(refreshToken);
			}
			expiresIn = 1800;
			expire1 = DateTime.Now.AddSeconds(expiresIn);
			expire2 = DateTime.Now.AddDays(90);
		}
		internal static Token Generate(Guid guid)
		{
			LoadTokens();
			Token token = new(guid);
			list.Add(token);
			SaveTokens();
			return token;
		}
		private static Token? GetToken(byte[] accessToken)
		{
			LoadTokens();
			Token? token = list.Where(x => Convert.ToBase64String(x.accessToken) == Convert.ToBase64String(accessToken)).FirstOrDefault();
			if (token != null)
			{
				if (DateTime.Now < token.expire1)
				{
					return token;
				}
				else
				{
					list.Remove(token);
				}
			}
			return null;
		}
		internal static Guid? Owner(byte[] accessToken)
		{
			Token? x = GetToken(accessToken);
			return x != null ? x.owner : null;
		}
		internal static Token? Refresh(byte[] refreshToken)
		{
			LoadTokens();
			Token? token = list.Where(x => Convert.ToBase64String(x.refreshToken) == Convert.ToBase64String(refreshToken)).FirstOrDefault();
			if (token != null && list.Contains(token))
			{
				list.Remove(token);
				SaveTokens();
				if (DateTime.Now < token.expire2)
				{
					Token generated = Generate(token.owner);
					return generated;
				}
				SaveTokens();
			}
			return null;
		}
		internal static void SaveTokens()
		{
			string path = "Base" + Path.DirectorySeparatorChar + "tokens.auram";
			Directory.CreateDirectory("Base");
			Database db = new();
			Token[] tokens = list.Where(x => DateTime.Now < x.expire1).ToArray();
			for (int i = 0; i < tokens.Length; i++)
			{
				Token token = tokens[i];
				db.Set(i.ToString(), token.owner.ToByteArray());
				db.Set(i + "/expire1", BitConverter.GetBytes(token.expire1.ToBinary()));
				db.Set(i + "/expire2", BitConverter.GetBytes(token.expire2.ToBinary()));
				db.Set(i + "/accessToken", token.accessToken);
				db.Set(i + "/refreshToken", token.refreshToken);
			}
			db.Save(path);
		}
		internal static void LoadTokens()
		{
			if (!loaded)
			{
				loaded = true;
				string path = "Base" + Path.DirectorySeparatorChar + "tokens.auram";
				if (File.Exists(path))
				{
					list.Clear();
					Database db = new(path);
					string[] keys = db.data.Keys.Where(x => !x.Contains('/')).ToArray();
					for (int i = 0; i < keys.Length; i++)
					{
						byte[]? owner = db.Get(i.ToString());
						byte[]? rawExpire1 = db.Get(i + "/expire1");
						byte[]? rawExpire2 = db.Get(i + "/expire2");
						byte[]? rawAccessToken = db.Get(i + "/accessToken");
						byte[]? rawRefreshToken = db.Get(i + "/refreshToken");
						if (owner != null && rawExpire1 != null && rawExpire2 != null && rawAccessToken != null && rawRefreshToken != null)
						{
							DateTime expire1 = DateTime.FromBinary(BitConverter.ToInt64(rawExpire1));
							DateTime expire2 = DateTime.FromBinary(BitConverter.ToInt64(rawExpire2));
							if (DateTime.Now < expire1 && DateTime.Now < expire2)
							{
								Token token = new(new Guid(owner));
								token.expire1 = expire1;
								token.expire2 = expire2;
								token.accessToken = rawAccessToken;
								token.refreshToken = rawRefreshToken;
								list.Add(token);
							}
						}
					}
				}
			}
		}
	}
}