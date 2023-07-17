using TTMC.Auram;

namespace TTMC.LoginSystem
{
	public class Token
	{
		private static bool loaded = false;
		private static List<Token> list = new();
		public Guid owner = Guid.Empty;
		public byte[] accessToken = new byte[64];
		public byte[] refreshToken = new byte[32];
		public DateTime expireAccess = DateTime.MinValue;
		public DateTime expireRefresh = DateTime.MinValue;
		private static Random random = new();
		public bool validAccess { get { return DateTime.Now < expireAccess; } }
		public bool validRefresh { get { return DateTime.Now < expireRefresh; } }
		public byte[] ToByteArray()
		{
			List<byte> list = new();
			list.AddRange(owner.ToByteArray());
			list.AddRange(accessToken);
			list.AddRange(refreshToken);
			list.AddRange(BitConverter.GetBytes(expireAccess.ToBinary()));
			list.AddRange(BitConverter.GetBytes(expireRefresh.ToBinary()));
			return list.ToArray();
		}
		public static Token FromByteArray(byte[] data)
		{
			Token token = new()
			{
				owner = new Guid(data[..16]),
				accessToken = data[16..80],
				refreshToken = data[80..112],
				expireAccess = DateTime.FromBinary(BitConverter.ToInt64(data, 112)),
				expireRefresh = DateTime.FromBinary(BitConverter.ToInt64(data, 120))
			};
			return token;
		}
		public static Token Create(Guid guid)
		{
			Token token = new() { owner = guid };
			random.NextBytes(token.accessToken);
			while (list.Any(x => x.accessToken == token.accessToken))
			{
				random.NextBytes(token.accessToken);
			}
			random.NextBytes(token.refreshToken);
			while (list.Any(x => x.refreshToken == token.refreshToken))
			{
				random.NextBytes(token.refreshToken);
			}
			token.expireAccess = DateTime.Now.AddSeconds(1800);
			token.expireRefresh = DateTime.Now.AddDays(90);
			return token;
		}
		internal static Token Generate(Guid guid)
		{
			LoadTokens();
			Token token = Create(guid);
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
				if (DateTime.Now < token.expireAccess)
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
				if (DateTime.Now < token.expireRefresh)
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
			Token[] tokens = list.Where(x => DateTime.Now < x.expireAccess).ToArray();
			for (int i = 0; i < tokens.Length; i++)
			{
				Token token = tokens[i];
				db.Set(i.ToString(), token.owner.ToByteArray());
				db.Set(i + "/expireAccess", BitConverter.GetBytes(token.expireAccess.ToBinary()));
				db.Set(i + "/expireRefresh", BitConverter.GetBytes(token.expireRefresh.ToBinary()));
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
						byte[]? owner = db.Get<byte[]>(i.ToString());
						byte[]? rawExpireAccess = db.Get<byte[]>(i + "/expireAccess");
						byte[]? rawExpireRefresh = db.Get<byte[]>(i + "/expireRefresh");
						byte[]? rawAccessToken = db.Get<byte[]>(i + "/accessToken");
						byte[]? rawRefreshToken = db.Get<byte[]>(i + "/refreshToken");
						if (owner != null && rawExpireAccess != null && rawExpireRefresh != null && rawAccessToken != null && rawRefreshToken != null)
						{
							DateTime expire1 = DateTime.FromBinary(BitConverter.ToInt64(rawExpireAccess));
							DateTime expire2 = DateTime.FromBinary(BitConverter.ToInt64(rawExpireRefresh));
							if (DateTime.Now < expire1 && DateTime.Now < expire2)
							{
								Token token = Create(new Guid(owner));
								token.expireAccess = expire1;
								token.expireRefresh = expire2;
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