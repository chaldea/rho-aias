using System.Security.Cryptography;

namespace Chaldea.Fate.RhoAias;

public class PasswordHasher
{
	private const int SaltSize = 16; // 盐值的大小，以字节为单位
	private const int HashSize = 20; // 哈希值的大小，以字节为单位
	private const int Iterations = 10000; // 迭代次数

	public static string HashPassword(string password)
	{
		// 生成随机的盐值
		byte[] salt;
		new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

		// 使用 PBKDF2 算法进行密码哈希
		var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
		var hash = pbkdf2.GetBytes(HashSize);

		// 将盐值和哈希值合并存储
		var hashBytes = new byte[SaltSize + HashSize];
		Array.Copy(salt, 0, hashBytes, 0, SaltSize);
		Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

		// 将字节数组转换为 Base64 字符串存储
		return Convert.ToBase64String(hashBytes);
	}

	public static bool VerifyPassword(string password, string hashedPassword)
	{
		// 从 Base64 字符串解析盐值和哈希值
		var hashBytes = Convert.FromBase64String(hashedPassword);
		var salt = new byte[SaltSize];
		Array.Copy(hashBytes, 0, salt, 0, SaltSize);
		var hash = new byte[HashSize];
		Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

		// 使用相同的盐值和迭代次数计算密码的哈希值
		var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
		var testHash = pbkdf2.GetBytes(HashSize);

		// 比较两个哈希值是否相等
		for (var i = 0; i < HashSize; i++)
			if (hash[i] != testHash[i])
				return false;
		return true;
	}
}

public class User
{
	public Guid Id { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }

	public void HashPassword(string pwd)
	{
		Password = PasswordHasher.HashPassword(pwd);
	}

	public bool VerifyPassword(string password)
	{
		return PasswordHasher.VerifyPassword(password, Password);
	}
}