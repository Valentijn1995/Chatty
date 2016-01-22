using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Chatty
{
    public static class SecurityManager {
        private const int KEY_SIZE = 512;

        private static bool _optimalAsymmetricEncryptionPadding = false;

        /// <summary>
        /// Converts string to SHA1 Hash.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SHA1hash(string input) {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        /// <summary>
        /// Generate a Keypair with RSA encryption.
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static KeyPair GenerateKeys(int keySize = KEY_SIZE) {
            if (!IsKeySizeValid(keySize))
                throw new Exception("Key should be multiple of two and greater than 512.");

            var response = new KeyPair();

            using (var provider = new RSACryptoServiceProvider(keySize)) {
                var publicKey = provider.ToXmlString(false);
                var privateKey = provider.ToXmlString(true);

                var publicKeyWithSize = IncludeKeyInEncryptionString(publicKey, keySize);
                var privateKeyWithSize = IncludeKeyInEncryptionString(privateKey, keySize);

                response.PublicKey = publicKeyWithSize;
                response.PrivateKey = privateKeyWithSize;
            }

            return response;
        }

        /// <summary>
        ///  Encrypts the text using the key
        /// </summary>
        /// <param name="text"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string EncryptText(string text, string publicKey) {
            int keySize = 0;
            string publicKeyXml = "";

            GetKeyFromEncryptionString(publicKey, out keySize, out publicKeyXml);

            try {
                var encrypted = Encrypt(Encoding.UTF8.GetBytes(text), keySize, publicKeyXml);
                return Convert.ToBase64String(encrypted);
            } catch {}
            return null;
        }

        /// <summary>
        /// Encrypts the text using the given keySize and key
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keySize"></param>
        /// <param name="publicKeyXml"></param>
        /// <returns></returns>
        private static byte[] Encrypt(byte[] data, int keySize, string publicKeyXml) {
            if(data == null || data.Length == 0)
                throw new ArgumentException("Data are empty", nameof(data));

            int maxLength = GetMaxDataLength(keySize);
            if (data.Length > maxLength)
                throw new ArgumentException(string.Format("Maximum data length is {0}", maxLength), nameof(data));

            if(!IsKeySizeValid(keySize))
                throw new ArgumentException("Key size is not valid", nameof(keySize));

            if(String.IsNullOrEmpty(publicKeyXml))
                throw new ArgumentException("Key is null or empty", nameof(publicKeyXml));

            using(var provider = new RSACryptoServiceProvider(keySize)) {
                provider.FromXmlString(publicKeyXml);
                return provider.Encrypt(data, _optimalAsymmetricEncryptionPadding);
            }
        }

        /// <summary>
        /// Decrypts the text using the key
        /// </summary>
        /// <param name="text"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string DecryptText(string text, string privateKey) {
            int keySize = 0;
            string publicAndPrivateKeyXml = "";

            GetKeyFromEncryptionString(privateKey, out keySize, out publicAndPrivateKeyXml);

            var decrypted = Decrypt(Convert.FromBase64String(text), keySize, publicAndPrivateKeyXml);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Decrypts the text using the given keySize and key
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keySize"></param>
        /// <param name="publicAndPrivateKeyXml"></param>
        /// <returns></returns>
        private static byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml) {
            if(data == null || data.Length == 0)
                throw new ArgumentException("Data are empty", nameof(data));

            if (!IsKeySizeValid(keySize))
                throw new ArgumentException("Key size is not valid", nameof(keySize));

            if (string.IsNullOrEmpty(publicAndPrivateKeyXml))
                throw new ArgumentException("Key is null or empty", nameof(publicAndPrivateKeyXml));

            using(var provider = new RSACryptoServiceProvider(keySize)) {
                provider.FromXmlString(publicAndPrivateKeyXml);
                return provider.Decrypt(data, _optimalAsymmetricEncryptionPadding);
            }
        }

        /// <summary>
        /// Returns the max length bases on keysize.
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static int GetMaxDataLength(int keySize = KEY_SIZE) {
            if (_optimalAsymmetricEncryptionPadding) {
                return ((keySize - 384) / 8) + 7;
            }
            return ((keySize - 384) / 8) + 37;
        }

        /// <summary>
        /// Checks wether the given keySize is a valid size.
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static bool IsKeySizeValid(int keySize) => keySize >= 384 &&
            keySize <= 16384 &&
            keySize % 8 == 0;

        /// <summary>
        /// Converts key to a usable string.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
        private static string IncludeKeyInEncryptionString(string publicKey, int keySize) => Convert.ToBase64String(Encoding.UTF8.GetBytes(keySize.ToString() + "!" + publicKey));

        /// <summary>
        /// Retrieve the key from the raw generated key.
        /// </summary>
        /// <param name="rawkey"></param>
        /// <param name="keySize"></param>
        /// <param name="xmlKey"></param>
        private static void GetKeyFromEncryptionString(string rawkey, out int keySize, out string xmlKey) {
            keySize = 0;
            xmlKey = "";

            if (rawkey != null && rawkey.Length > 0) {
                byte[] keyBytes = Convert.FromBase64String(rawkey);
                var stringKey = Encoding.UTF8.GetString(keyBytes);

                if (stringKey.Contains("!")) {
                    var splittedValues = stringKey.Split(new char[] { '!' }, 2);

                    try {
                        keySize = int.Parse(splittedValues[0]);
                        xmlKey = splittedValues[1];
                    }
                    catch { }
                }
            }
        }
    }

    [Serializable]
    public class KeyPair
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}

