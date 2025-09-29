// Services/ECCService.cs (Corrected for Warnings)
using System.Security.Cryptography;
using System.Text;

namespace SecureCommECC.Services
{
    public class ECCService
    {
        public (byte[] privateKey, byte[] publicKey) GenerateNewKeyPair()
        {
            using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            byte[] privateKey = ecdh.ExportPkcs8PrivateKey();
            byte[] publicKey = ecdh.ExportSubjectPublicKeyInfo();
            return (privateKey, publicKey);
        }

        public byte[] Encrypt(string plaintext, byte[] myPrivateKey, byte[] recipientPublicKeyBytes)
        {
            using var myKey = ECDiffieHellman.Create();
            myKey.ImportPkcs8PrivateKey(myPrivateKey, out _);

            using var recipientPublicKey = ECDiffieHellman.Create();
            recipientPublicKey.ImportSubjectPublicKeyInfo(recipientPublicKeyBytes, out _);

            byte[] sharedSecret = myKey.DeriveKeyMaterial(recipientPublicKey.PublicKey);
            byte[] symmetricKey = DeriveSymmetricKeyFromSecret(sharedSecret);

            // FIX: Using the modern, non-obsolete constructor
            using AesGcm aesGcm = new(symmetricKey);
            byte[] iv = new byte[12]; // AesGcm.NonceByteSizes.MaxSize
            RandomNumberGenerator.Fill(iv);
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[dataToEncrypt.Length];
            byte[] tag = new byte[16]; // AesGcm.TagByteSizes.MaxSize

            aesGcm.Encrypt(iv, dataToEncrypt, ciphertext, tag);

            byte[] result = new byte[iv.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, result, iv.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, iv.Length + ciphertext.Length, tag.Length);

            return result;
        }

        public string Decrypt(byte[] encryptedData, byte[] myPrivateKey, byte[] senderPublicKeyBytes)
        {
            using var myKey = ECDiffieHellman.Create();
            myKey.ImportPkcs8PrivateKey(myPrivateKey, out _);
            
            using var senderPublicKey = ECDiffieHellman.Create();
            senderPublicKey.ImportSubjectPublicKeyInfo(senderPublicKeyBytes, out _);

            byte[] sharedSecret = myKey.DeriveKeyMaterial(senderPublicKey.PublicKey);
            byte[] symmetricKey = DeriveSymmetricKeyFromSecret(sharedSecret);

            byte[] iv = new byte[12];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
            byte[] tag = new byte[16];
            Buffer.BlockCopy(encryptedData, encryptedData.Length - tag.Length, tag, 0, tag.Length);
            byte[] ciphertext = new byte[encryptedData.Length - iv.Length - tag.Length];
            Buffer.BlockCopy(encryptedData, iv.Length, ciphertext, 0, ciphertext.Length);

            // FIX: Using the modern, non-obsolete constructor
            using AesGcm aesGcm = new(symmetricKey);
            byte[] plaintextBytes = new byte[ciphertext.Length];
            aesGcm.Decrypt(iv, ciphertext, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private byte[] DeriveSymmetricKeyFromSecret(byte[] sharedSecret)
        {
            using SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(sharedSecret);
        }
    }
}