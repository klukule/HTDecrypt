using System.Security.Cryptography;
using System.Text;

namespace HTCrypto;

public static class HTCipher
{
    public const string SplitToken = "|SPLIT|";
    public const int BlockSize = 16;

    private static readonly UTF8Encoding Utf8Strict = new(false, throwOnInvalidBytes: true);

    public static byte[] Encrypt(byte[] plaintext, KeyId keyId)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = KnownKeys.Get(keyId);
        return aes.EncryptEcb(plaintext, PaddingMode.PKCS7);
    }

    public static string EncryptToBase64(string plaintext, KeyId keyId)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        return Convert.ToBase64String(Encrypt(bytes, keyId));
    }

    public static byte[] Decrypt(byte[] ciphertext, KeyId keyId)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = KnownKeys.Get(keyId);
        return aes.DecryptEcb(ciphertext, PaddingMode.PKCS7);
    }

    public static string? TryDecryptBase64Line(string base64, out KeyId? matchedKey)
    {
        matchedKey = null;
        if (!LooksLikeBase64(base64))
            return null;

        byte[] cipher;
        try
        {
            cipher = Convert.FromBase64String(base64);
        }
        catch
        {
            return null;
        }

        if (cipher.Length == 0 || cipher.Length % BlockSize != 0)
            return null;

        foreach (var keyId in KnownKeys.AutoDetectOrder)
        {
            try
            {
                var plain = Decrypt(cipher, keyId);
                try
                {
                    var text = Utf8Strict.GetString(plain);
                    matchedKey = keyId;
                    return text;
                }
                catch (DecoderFallbackException)
                {
                    // Decrypted to non-UTF-8 garbage - wrong key for this line.
                }
            }
            catch (CryptographicException)
            {
                // PKCS7 padding wasn't valid - wrong key for this line.
            }
        }

        return null;
    }

    public static bool LooksLikeBase64(string s)
    {
        if (s.Length < 4 || s.Length % 4 != 0)
            return false;

        foreach (var c in s)
        {
            bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=';
            if (!ok) return false;
        }

        return true;
    }
}