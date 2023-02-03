using System.Security.Cryptography;
using System.Text;

namespace TTMC.LoginSystem
{
	public class Encryption
	{
		private static SHA256 sha = SHA256.Create();
		private static Aes aes = Aes.Create();
		private static MD5 md5 = MD5.Create();
		public static byte[] Encrypt(byte[] data , string key)
		{
			aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
			return aes.EncryptCfb(data, md5.ComputeHash(Encoding.UTF8.GetBytes("TTMC LoginSystem 4")));
		}
		public static byte[] Decrypt(byte[] data, string key)
		{
			aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
			return aes.DecryptCfb(data, md5.ComputeHash(Encoding.UTF8.GetBytes("TTMC LoginSystem 4")));
		}
	}
}