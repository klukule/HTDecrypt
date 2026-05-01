using System.Text;

namespace HTCrypto;

public static class HTLineProcessor
{
    public static DecryptResult DecryptLines(IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        int decryptedBlocks = 0;
        int passthroughLines = 0;
        var keysSeen = new HashSet<KeyId>();

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();
            if (trimmed.Length == 0)
            {
                sb.AppendLine();
                continue;
            }

            var decoded = HTCipher.TryDecryptBase64Line(trimmed, out var matchedKey);
            if (decoded is null)
            {
                sb.AppendLine(rawLine);
                passthroughLines++;
                continue;
            }

            if (matchedKey.HasValue) keysSeen.Add(matchedKey.Value);

            var expanded = decoded.Replace(HTCipher.SplitToken, Environment.NewLine);
            expanded = expanded.TrimEnd('\r', '\n');
            sb.AppendLine(expanded);
            decryptedBlocks++;
        }

        return new DecryptResult(sb.ToString(), decryptedBlocks, passthroughLines, keysSeen);
    }

    public static EncryptResult EncryptLines(IEnumerable<string> lines, KeyId keyId)
    {
        var sb = new StringBuilder();
        int contentLines = 0;
        int blankMarkers = 0;
        var splitTokenBytes = Encoding.ASCII.GetBytes(HTCipher.SplitToken);

        foreach (var line in lines)
        {
            byte[] plaintext;
            if (string.IsNullOrEmpty(line))
            {
                plaintext = splitTokenBytes;
                blankMarkers++;
            }
            else
            {
                plaintext = Encoding.UTF8.GetBytes(line);
                contentLines++;
            }

            var ct = HTCipher.Encrypt(plaintext, keyId);
            sb.Append(Convert.ToBase64String(ct));
            sb.Append("\r\n");
        }

        return new EncryptResult(sb.ToString(), contentLines, blankMarkers);
    }

    public readonly record struct DecryptResult(string Text, int DecryptedBlocks, int PassthroughLines, IReadOnlyCollection<KeyId> KeysUsed);
    public readonly record struct EncryptResult(string Text, int ContentLines, int BlankLineMarkers);
}
