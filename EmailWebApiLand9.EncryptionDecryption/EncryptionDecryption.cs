using System.Security.Cryptography;
using System.Text;

namespace EmailWebApiLand9.EncryptionDecryption
{
    public static class AES
    {
        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new())
            {
                using var AES = Aes.Create(); // This reports Warning  SYSLIB0022  'RijndaelManaged' is obsolete: 'The Rijndael and RijndaelManaged types are obsolete. Use Aes instead.
                AES.KeySize = 256;
                AES.BlockSize = 128;
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 5000, HashAlgorithmName.SHA256);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;
                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }
                encryptedBytes = ms.ToArray();
            }
            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new())
            {
                using var AES = Aes.Create();  // This reports Warning  SYSLIB0022  'RijndaelManaged' is obsolete: 'The Rijndael and RijndaelManaged types are obsolete. Use Aes instead.
                AES.KeySize = 256;
                AES.BlockSize = 128;
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 5000, HashAlgorithmName.SHA256);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;
                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }
            return decryptedBytes;
        }

        public static string EncryptText(string password, string salt = "WhenYouWishUponAStarMakesNoDiffernc")
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(password);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(salt);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
            string result = Convert.ToBase64String(bytesEncrypted);
            return result;
        }

        public static string DecryptText(string hash, string salt = "WhenYouWishUponAStarMakesNoDiffernc")
        {
            try
            {
                byte[] bytesToBeDecrypted = Convert.FromBase64String(hash);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(salt);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
                byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);
                string result = Encoding.UTF8.GetString(bytesDecrypted);
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private const int SALT_BYTE_SIZE = 24;
        private const int HASH_BYTE_SIZE = 24;
        private const int PBKDF2_ITERATIONS = 1000;
        private const int ITERATION_INDEX = 0;
        private const int SALT_INDEX = 1;
        private const int PBKDF2_INDEX = 2;

        public static string PBKDF2_CreateHash(string password)
        {

            var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SALT_BYTE_SIZE];
            rng.GetBytes(salt);
            byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
            return PBKDF2_ITERATIONS + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool PBKDF2_ValidatePassword(string password, string correctHash)
        {
            char[] delimiter = { ':' };
            string[] split = correctHash.Split(delimiter);
            int iterations = Int32.Parse(split[ITERATION_INDEX]);
            byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
            byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);
            byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new(password, salt, 5000, HashAlgorithmName.SHA256)
            {
                IterationCount = iterations
            };
            return pbkdf2.GetBytes(outputBytes);
        }
    }

}
