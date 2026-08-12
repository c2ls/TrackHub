// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Security.Cryptography;
using System.Text;

namespace Common.Domain.Extensions;

// Contains extension methods for cryptographic operations.
public static class CryptographyExtensions
{
    // Hashes a password using BCrypt hashing algorithm.
    public static string HashPassword(this string value)
        => BCrypt.Net.BCrypt.HashPassword(value);

    // Verifies a hashed password against a plain text password.
    public static bool VerifyHashedPassword(this string hashedPassword, string password)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);

    // Generates a random AES key of the specified key size.
    public static byte[] GenerateAesKey(int keySizeBits)
    {
        if (keySizeBits != 128 && keySizeBits != 192 && keySizeBits != 256)
            throw new ArgumentException("Invalid key size. Valid sizes are 128, 192, or 256 bits.", nameof(keySizeBits));

        byte[] key = new byte[keySizeBits / 8];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    // Derives a key from a passphrase and salt using the Rfc2898DeriveBytes algorithm.
    public static byte[] DeriveKey(string passphrase, byte[] salt, int keySize = 256, int iterations = 100000)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password: passphrase,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: keySize / 8
        );
    }

    // Encrypts the given data using AES encryption algorithm.
    public static string EncryptData(this string dataToEncrypt, string passphrase, byte[] salt)
    {
        byte[] dataToEncryptBytes = Encoding.UTF8.GetBytes(dataToEncrypt);
        byte[] key = DeriveKey(passphrase, salt);

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.GenerateIV();
        aesAlg.Padding = PaddingMode.PKCS7;
        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(dataToEncryptBytes, 0, dataToEncryptBytes.Length);
        csEncrypt.FlushFinalBlock();

        var encryptedData = aesAlg.IV.Concat(msEncrypt.ToArray()).ToArray();
        return Convert.ToBase64String(encryptedData);
    }

    // Decrypts the given encrypted data using AES encryption algorithm.
    public static string DecryptData(this string encryptedDataWithIvBase64, string passphrase, byte[] salt)
    {
        byte[] encryptedDataWithIv = Convert.FromBase64String(encryptedDataWithIvBase64);
        byte[] iv = [.. encryptedDataWithIv.Take(16)];
        byte[] encryptedData = [.. encryptedDataWithIv.Skip(16)];

        byte[] key = DeriveKey(passphrase, salt);

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;
        aesAlg.Padding = PaddingMode.PKCS7;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(encryptedData);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msResult = new MemoryStream();
        csDecrypt.CopyTo(msResult);

        return Encoding.UTF8.GetString(msResult.ToArray());
    }
}
