using System.Text;

namespace HTCrypto;

public static class KnownKeys
{
    public static readonly byte[] Windows = Convert.FromBase64String("VVZiUDZwamp3NUtaaHZkZGllM3RmaGcxcFZra3ZlWTg=");
    public static readonly byte[] IOS = Convert.FromBase64String("I2J3M0ZaFfHVO1Z8ZLIEcw=="); // NOTE: Untested

    public static byte[] Get(KeyId id) => id switch
    {
        KeyId.Windows => Windows,
        KeyId.IOS => IOS,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
    };

    public static readonly IReadOnlyList<KeyId> AutoDetectOrder =
    [
        KeyId.Windows,
        KeyId.IOS
    ];
}
