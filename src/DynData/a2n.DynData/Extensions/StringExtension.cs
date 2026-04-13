using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class StringExtension
    {
        public static string ToHumanReadable(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            var len = value.Length;
            var sb = new StringBuilder(len + len / 2);

            for (var i = 0; i < len; i++)
            {
                var c = value[i];

                if (i == 0)
                {
                    sb.Append(char.ToUpper(c));
                }
                else if (char.IsUpper(c))
                {
                    sb.Append(' ');
                    sb.Append(c);
                }
                else if (c == '-' || c == '_')
                {
                    sb.Append(' ');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            if (value.Length == 1)
                return value.ToLower();
            return string.Format("{0}{1}", Char.ToLowerInvariant(value[0]), value.Substring(1));
        }
        public static string EncryptWithCertificate(StoreName storeName, StoreLocation storeLocation, string subject, string clearText)
        {
            if (string.IsNullOrEmpty(clearText))
                throw new ArgumentNullException(nameof(clearText));
            using X509Store store = new X509Store(storeName, storeLocation, OpenFlags.OpenExistingOnly);
            var subjectName = $"CN={subject}";
            var cert = store.Certificates
                .Where(t => t.Subject.ToLower() == subjectName.ToLower())
                .FirstOrDefault();
            if (cert == null)
                throw new Exception($"Certificate not found: {subjectName}");
            var encrypted64Text = string.Empty;

            using (RSA rsaPub = cert.GetRSAPublicKey())
            {
                var dataBuffer = Encoding.UTF8.GetBytes(clearText);
                var encryptedDataBuffer = rsaPub.Encrypt(dataBuffer, RSAEncryptionPadding.Pkcs1);
                encrypted64Text = Convert.ToBase64String(encryptedDataBuffer);
            }
            return encrypted64Text;
        }
        public static string DecryptWithCertificate(StoreName storeName, StoreLocation storeLocation, string subject, string encrypted64Text)
        {
            if (string.IsNullOrEmpty(encrypted64Text))
                throw new ArgumentNullException(nameof(encrypted64Text));
            using X509Store store = new X509Store(storeName, storeLocation, OpenFlags.OpenExistingOnly);
            var subjectName = $"CN={subject}";
            var cert = store.Certificates
                .Where(t => t.Subject.ToLower() == subjectName.ToLower())
                .FirstOrDefault();
            if (cert == null)
                throw new Exception($"Certificate not found: {subjectName}");
            var decryptText = string.Empty;

            using (RSA rsaPriv = cert.GetRSAPrivateKey())
            {
                var dataBuffer = Convert.FromBase64String(encrypted64Text);
                var decryptedDataBuffer = rsaPriv.Decrypt(dataBuffer, RSAEncryptionPadding.Pkcs1);
                decryptText = Encoding.UTF8.GetString(decryptedDataBuffer);
            }
            return decryptText;
        }
    }
}
