using System.Text;

namespace HTCrypto;

public static class KnownKeys
{
    /// <summary>
    /// Global client - Windows
    /// </summary>
    public static readonly byte[] Windows = Convert.FromBase64String("VVZiUDZwamp3NUtaaHZkZGllM3RmaGcxcFZra3ZlWTg=");

    /// <summary>
    /// Mainland China client - Windows
    /// </summary>
    public static readonly byte[] WindowsCN = Convert.FromBase64String("MXpoNklPbElvaHJSODhVTlBqaUxpc3JrV0FDVVFZdXo=");

    public static byte[] Get(KeyId id) => id switch
    {
        KeyId.Windows => Windows,
        KeyId.WindowsCN => WindowsCN,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
    };

    public static readonly IReadOnlyList<KeyId> AutoDetectOrder =
    [
        KeyId.Windows,
        KeyId.WindowsCN
    ];
}
