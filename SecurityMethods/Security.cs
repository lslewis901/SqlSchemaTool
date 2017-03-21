using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace Lewis.SST.SecurityMethods
{
    /// <summary>
    /// Security class handles encryption/decryption for connection strings
    /// </summary>
    public class Security
    {
        /// <summary>
        /// Call this function to remove the key from memory after use for security.
        /// </summary>
        /// <param name="Destination"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);

        /// <summary>
        /// Function to Generate 64 bit Key.
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }

        private Security()
        {
            // default ctor
        }

        /// <summary>
        /// static method to encrypt passed in string using passed in public key
        /// </summary>
        /// <param name="plainString"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string EncryptString(string plainString, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            String outstring = null;
            using (MemoryStream sEncrypted = new MemoryStream())
            {
                byte[] output = null;
                ICryptoTransform desencrypt = DES.CreateEncryptor();
                using (CryptoStream cryptostream = new CryptoStream(sEncrypted, desencrypt, CryptoStreamMode.Write))
                {
                    UTF8Encoding e = new UTF8Encoding();
                    byte[] bytearrayinput = e.GetBytes(plainString.ToCharArray());
                    cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                    cryptostream.FlushFinalBlock();
                    cryptostream.Close();
                }
                output = sEncrypted.ToArray();
                sEncrypted.Close();
                outstring = Convert.ToBase64String(output);
            }
            
            return  outstring;
        }

        /// <summary>
        /// static method to decrypt passed in string using passed in public key
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string DecryptString(string encryptedString, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            string outstring = null;
            using (MemoryStream sDecrypted = new MemoryStream())
            {
                try
                {
                    byte[] output = null;
                    UTF8Encoding e = new UTF8Encoding();
                    ICryptoTransform desdecrypt = DES.CreateDecryptor();
                    using (CryptoStream cryptostream = new CryptoStream(sDecrypted, desdecrypt, CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = Convert.FromBase64String(encryptedString);
                        cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                        cryptostream.FlushFinalBlock();
                        cryptostream.Close();
                    }
                    output = sDecrypted.ToArray();
                    sDecrypted.Close();
                    outstring = e.GetString(output);
                }
                catch //(CryptographicException cex)
                {
                    // nop 
                }
            }
            return outstring;
        }
    }
}
