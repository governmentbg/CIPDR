using URegister.Core.Constants.DGC;
using NodaTime;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PeterO.Cbor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace URegister.Core.Utils
{
    /// <summary>
    /// General Crypto methods
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Path to test certificates
        /// </summary>
        private static string PathToCertificates = @"Certificates/";

        public static void SetCertificatePath(string path)
        {
            PathToCertificates = path;
        }

        /// <summary>
        /// Encodes byte array to HEX string
        /// </summary>
        /// <param name="bytes">Byte array to be encoded</param>
        /// <returns>HEX encoded string</returns>
        public static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes HEX encoded string to byte array
        /// </summary>
        /// <param name="hex">HEX encoded string</param>
        /// <returns>Decoded byte array</returns>
        public static byte[] FromHexString(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        public static byte[] ConvertConcatToDer(byte[] concat)
        {
            int len = concat.Length / 2;

            byte[] r = new byte[len];
            Array.Copy(concat, 0, r, 0, len);
            r = UnsignedInteger(r);

            byte[] s = new byte[len];
            Array.Copy(concat, len, s, 0, len);
            s = UnsignedInteger(s);

            var x = new List<byte[]>();
            x.Add(new byte[] { 0x30 });
            x.Add(new byte[] { (byte)(r.Length + s.Length) });
            x.Add(r);
            x.Add(s);

            var der = x.SelectMany(p => p).ToArray();
            return der;
        }

        private static byte[] UnsignedInteger(byte[] i)
        {
            int pad = 0, offset = 0;

            while (offset < i.Length && i[offset] == 0)
            {
                offset++;
            }

            if (offset == i.Length)
            {
                return new byte[] { 0x02, 0x01, 0x00 };
            }
            if ((i[offset] & 0x80) != 0)
            {
                pad++;
            }

            int length = i.Length - offset;
            byte[] der = new byte[2 + length + pad];
            der[0] = 0x02;
            der[1] = (byte)(length + pad);
            Array.Copy(i, offset, der, 2 + pad, length);

            return der;
        }

        public static byte[] ConvertDerToConcat(byte[] der, int len)
        {
            // this is far too naive
            byte[] concat = new byte[len * 2];

            // assumes SEQUENCE is organized as "R + S"
            int kLen = 4;
            if (der[0] != 0x30)
            {
                throw new Exception("Unexpected signature input");
            }
            if ((der[1] & 0x80) != 0)
            {
                // offset actually 4 + (7-bits of byte 1)
                kLen = 4 + (der[1] & 0x7f);
            }

            // calculate start/end of R
            int rOff = kLen;
            int rLen = der[rOff - 1];
            int rPad = 0;
            if (rLen > len)
            {
                rOff += (rLen - len);
                rLen = len;
            }
            else
            {
                rPad = (len - rLen);
            }
            // copy R
            Array.Copy(der, rOff, concat, rPad, rLen);

            // calculate start/end of S
            int sOff = rOff + rLen + 2;
            int sLen = der[sOff - 1];
            int sPad = 0;
            if (sLen > len)
            {
                sOff += (sLen - len);
                sLen = len;
            }
            else
            {
                sPad = (len - sLen);
            }
            // copy S
            Array.Copy(der, sOff, concat, len + sPad, sLen);

            return concat;
        }

        /// <summary>
        /// Calculates COSE signature
        /// </summary>
        /// <param name="coseProtected">COSE Protected header</param>
        /// <param name="cosePayload">COSE Payload</param>
        /// <returns></returns>
        public static CBORObject GetSignature(CBORObject coseProtected, CBORObject cosePayload, bool stoicho = false)
        {
            CBORObject signObj = CBORObject.NewArray()
                .Add(SignatureContext.Signature1)
                .Add(coseProtected)
                .Add(new byte[0])
                .Add(cosePayload);

            AsymmetricKeyParameter privateKey = GetKey(stoicho);
            byte[] signature = SignData(signObj.EncodeToBytes(), privateKey);
            signature = ConvertDerToConcat(signature, 32);

            return CBORObject.FromObject(signature);
        }

        /// <summary>
        /// Sign data with ECDSA256 algorithm
        /// </summary>
        /// <param name="data">Data to be signed</param>
        /// <param name="privateKey">Signer certificate Private key</param>
        /// <returns></returns>
        public static byte[] SignData(byte[] data, AsymmetricKeyParameter privateKey)
        {
            var signer = SignerUtilities.GetSigner(CryptoAlgorithms.ECDSA256);

            signer.Init(true, privateKey);

            signer.BlockUpdate(data, 0, data.Length);

            return signer.GenerateSignature();
        }

        /// <summary>
        /// Gets the Public key Identifier 
        /// Needed to identify public key in trusted keys list
        /// Used only in production
        /// </summary>
        /// <returns>First 8 bytes of certificate hash</returns>
        public static byte[] GetKeyIdentifier(bool stoicho = false)
        {
            byte[] kid = null;
            string path = stoicho ? $"{PathToCertificates}dgc-dsc-4.pem" : $"{PathToCertificates}dgc-dsc-1.pem";

            var certificateString = File.ReadAllText(path);
            using (var textReader = new StringReader(certificateString))
            {
                Org.BouncyCastle.X509.X509Certificate bcCertificate = (Org.BouncyCastle.X509.X509Certificate)new PemReader(textReader).ReadObject();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] digest = sha256.ComputeHash(bcCertificate.GetEncoded());
                    kid = digest
                        .Take(8)
                        .ToArray();
                }
            }

            return kid;
        }

        /// <summary>
        /// Validates COSE signature
        /// </summary>
        /// <param name="cose">COSE object</param>
        /// <returns></returns>
        public static bool ValidateSignature(CBORObject cose, byte[] cert)
        {
            CBORObject signData = CBORObject.NewArray()
                .Add(SignatureContext.Signature1)
                .Add(cose[0])
                .Add(new byte[0])
                .Add(cose[2]);

            byte[] data = signData.EncodeToBytes();
            byte[] signature = cose[3].GetByteString();

            CBORObject header = CBORObject.DecodeFromBytes(cose[0].GetByteString());
            string algorithm = CryptoAlgorithms.RSA256;

            if (header[HeaderConstants.alg].AsInt32() == -7)
            {
                algorithm = CryptoAlgorithms.ECDSA256;
                signature = ConvertConcatToDer(signature);
            }

            AsymmetricKeyParameter publicKey = GetPublicKey(cert);

            ISigner signer = SignerUtilities.GetSigner(algorithm);
            signer.Init(false, publicKey);
            signer.BlockUpdate(data, 0, data.Length);
            
            return signer.VerifySignature(signature);
        }

        /// <summary>
        /// Validates signature
        /// </summary>
        /// <param name="data">Signed Data</param>
        /// <param name="signature">Signature</param>
        /// <param name="pubKey">Public key</param>
        /// <returns></returns>
        public static bool ValidateSignature(byte[] data, byte[] signature, byte[] pubKey)
        {
            string algorithm = CryptoAlgorithms.ECDSA512;
            AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(pubKey);
            ISigner signer = SignerUtilities.GetSigner(algorithm);
            signer.Init(false, publicKey);
            signer.BlockUpdate(data, 0, data.Length);
            
            return signer.VerifySignature(signature);
        }

        /// <summary>
        /// Gets Signer's certificate key pair
        /// </summary>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetKey(bool stoicho = false)
        {

            AsymmetricKeyParameter privateKey;
            string path = stoicho ? $"{PathToCertificates}dgc-dsc-4.key" : $"{PathToCertificates}dgc-dsc-1.key";
            var privateKeyString = File.ReadAllText(path);

            using (var textReader = new StringReader(privateKeyString))
            {
                // Only a private key
                privateKey = (AsymmetricKeyParameter)new PemReader(textReader).ReadObject();
            }

            return privateKey;

        }

        /// <summary>
        /// Gets CBOR Digest for External signing
        /// To be used with HSM in production
        /// </summary>
        /// <param name="payload">Object to be signed</param>
        /// <param name="keyIdentifier">Public key Identifier</param>
        /// <returns></returns>
        //public static byte[] GetDigest(byte[] payload, byte[] keyIdentifier)
        //{
        //    byte[] bytesToBeSigned = DGCHelper.GetDataToSign(payload, keyIdentifier)
        //        .EncodeToBytes();

        //    IDigest digest = new Sha256Digest();

        //    digest.BlockUpdate(bytesToBeSigned, 0, bytesToBeSigned.Length);
        //    byte[] digestedMessage = new byte[digest.GetDigestSize()];
        //    digest.DoFinal(digestedMessage, 0);

        //    return digestedMessage;
        //}

        /// <summary>
        /// To be implemented for External signature
        /// </summary>
        /// <param name="digest">Digest to be signed</param>
        /// <returns>Signature bytes</returns>
        //static byte[] SignDigest(byte[] digest)
        //{
        //    throw new NotImplementedException();
        //}

        static AsymmetricKeyParameter GetPublicKey(byte[] cert)
        {
            X509CertificateParser certParser = new X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate certificate = certParser.ReadCertificate(cert);

            return certificate.GetPublicKey();
        }

        public static string GetHmac(byte[] data, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(FromHexString(secret)))
            {
                byte[] computedHash = hmac.ComputeHash(data);

                return ToHexString(computedHash);
            }
        }

        public static bool CheckHmac(byte[] data, string hash, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(FromHexString(secret)))
            {
                byte[] computedHash = hmac.ComputeHash(data);

                return hash == ToHexString(computedHash);
            }
        }

        public static string GetHash(byte[] data, HashAlgorithmName hashName)
        {
            HashAlgorithmName[] allowedAlgorithms = new HashAlgorithmName[]
            {
                HashAlgorithmName.SHA256,
                HashAlgorithmName.SHA384,
                HashAlgorithmName.SHA512
            };

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!allowedAlgorithms.Any(x => x == hashName))
            {
                throw new NotSupportedException("Hash algoritm not supported");
            }

            string hash = null;

            if (hashName == HashAlgorithmName.SHA256)
            {
                using SHA256 sha256 = SHA256.Create();
                hash = ToHexString(sha256.ComputeHash(data));
            }
            else if (hashName == HashAlgorithmName.SHA384)
            {
                using SHA384 sha384 = SHA384.Create();
                hash = ToHexString(sha384.ComputeHash(data));
            }
            else if (hashName == HashAlgorithmName.SHA512)
            {
                using SHA512 sha512 = SHA512.Create();
                hash = ToHexString(sha512.ComputeHash(data));
            }

            return hash;
        }

        /// <summary>
        /// Връща информация за подписа от CMS контейнер
        /// </summary>
        /// <param name="container">CMS контейнер</param>
        /// <returns>Подписващ сертификат и време на подписване</returns>
        public static (X509Certificate2 certificate, DateTime? signingTime) GetSignatureInfoFromCMS(byte[] container)
        {
            SignedCms signedCms = new SignedCms();
            signedCms.Decode(container);
            DateTime? signingTime = null;
            DateTime dt;

            foreach (var signerInfo in signedCms.SignerInfos)
            {
                foreach (var attribute in signerInfo.SignedAttributes)
                {
                    if (attribute.Oid.Value == "1.2.840.113549.1.9.5")
                    {
                        Pkcs9SigningTime st = new Pkcs9SigningTime(attribute.Values[0].RawData);
                        signingTime = st.SigningTime;

                        break;
                    }
                }
            }

            if (signingTime != null)
            {
                dt = signingTime.Value.ToUniversalTime();
                var bgTimeZone = DateTimeZoneProviders.Tzdb["Europe/Sofia"];
                dt = Instant.FromDateTimeUtc(dt)
                              .InZone(bgTimeZone)
                              .ToDateTimeUnspecified();
                signingTime = dt;
            }

            return (signedCms.Certificates.FirstOrDefault(), signingTime);
        }

        /// <summary>
        /// Генериране на парола
        /// </summary>
        /// <param name="length">Дължина на паролата в байтове</param>
        /// <returns></returns>
        public static string GetSecret(int length)
        {
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            byte[] otp = new byte[length];
            random.NextBytes(otp);
            string otpStr = Convert.ToBase64String(otp);
            otpStr = otpStr
                .Replace("/", "_")
                .Replace("+","-");

            return otpStr;
        }
    }
}
