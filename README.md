# HTDecrypt

Set of tools to decrypt/encrypt and view logs and ini files for the game **Neverness To Everness**

Built using .NET 10 so build the tools using `dotnet build` etc.

## Usage

### Decrypt

```cmd
HTDecrypt.exe "C:\path\to\Engine.ini"
```

Or drag & drop one or more files onto the exe. Output:

```
C:\path\to\Engine.decrypted.ini
```

### Encrypt

```cmd
HTEncrypt.exe "C:\path\to\Engine.txt"
HTEncrypt.exe --key=ios "C:\path\to\IOSConfig.ini"
```

Default key is Windows (AES-256). Use `--key=ios` for AES-128 iOS-format files.

### Live tail

```cmd
HTLogTail.exe
```

...tails `%LOCALAPPDATA%\HT\Saved_Global\Logs\HT.log` by default (the live game log). Or:

```cmd
HTLogTail.exe "D:\some\HT-backup-2026.01.01.log"
```

Press `Ctrl+C` to exit.

## Keys

Built-in keys are for iOS (AES-128) and Windows (AES-256) although iOS one is unconfirmed... if you have binaries for other platforms (such as PS5, Android or MAC) feel free to open an issue and attach the binary so we can add support for other platforms as well - ideally also include config file or log file so we can confirm decryption works fine

The keys were dumped from 1.0.8 version and are confirmed working as of version 1.0.9 on windows