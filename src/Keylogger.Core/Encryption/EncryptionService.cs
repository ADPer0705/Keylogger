using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Keylogger.Core.Encryption
{
    /// <summary>
    /// Provides encryption and decryption services for sensitive data
    /// For educational and research purposes only
    /// </summary>
    public class EncryptionService
    {
        private readonly string _encryptionKey;

        public EncryptionService(string encryptionKey)
        {
            _encryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
        }

        /// <summary>
        /// Encrypts a string using AES encryption
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.IV = new byte[16]; // Initialization vector (set to zeros for simplicity)

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts an AES encrypted string
        /// </summary>
        /// <param name="cipherText">The Base64 encoded encrypted text</param>
        /// <returns>The original plaintext</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
                
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.IV = new byte[16]; // Same initialization vector as encryption

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}