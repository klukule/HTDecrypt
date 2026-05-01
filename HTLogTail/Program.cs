using System.Text;
using System.Text.RegularExpressions;
using HTCrypto;

namespace HTLogTail;

internal static class Program
{
    private const int PollIntervalMs = 200;

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var path = args.Length > 0 ? args[0] : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HT", "Saved_Global", "Logs", "HT.log");

        if (!WaitForFile(path, totalWaitMs: 30_000))
        {
            Console.Error.WriteLine($"File not found (waited 30s): {path}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: HTLogTail.exe [<path-to-encrypted-log>]");
            Console.Error.WriteLine(@"Default: %LOCALAPPDATA%\HT\Saved_Global\Logs\HT.log");
            return 1;
        }

        PrintHeader(path);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            TailForever(path, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // expected on Ctrl+C
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"[FATAL] {ex}");
            return 2;
        }

        return 0;
    }

    private static bool WaitForFile(string path, int totalWaitMs)
    {
        var deadline = Environment.TickCount64 + totalWaitMs;
        while (!File.Exists(path))
        {
            if (Environment.TickCount64 > deadline) return false;
            Thread.Sleep(500);
        }
        return true;
    }

    private static void TailForever(string path, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var fs = OpenShared(path);
            using var reader = new StreamReader(fs, detectEncodingFromByteOrderMarks: true);
            var carry = new StringBuilder();
            long handleSize = fs.Length;

            ConsumeAvailable(reader, carry);

            while (!ct.IsCancellationRequested)
            {
                ct.WaitHandle.WaitOne(PollIntervalMs);
                if (ct.IsCancellationRequested) break;

                long pathSize;
                try { pathSize = new FileInfo(path).Length; }
                catch { continue; }

                if (pathSize < handleSize)
                {
                    NotifyRotation();
                    break;
                }

                long currentHandleSize;
                try { currentHandleSize = fs.Length; }
                catch { break; }

                if (currentHandleSize > handleSize)
                {
                    ConsumeAvailable(reader, carry);
                    handleSize = currentHandleSize;
                }
            }
        }
    }

    private static FileStream OpenShared(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, bufferSize: 8192, options: FileOptions.SequentialScan);
    }

    private static void ConsumeAvailable(StreamReader reader, StringBuilder carry)
    {
        var buf = new char[16 * 1024];
        int read;
        while ((read = reader.Read(buf, 0, buf.Length)) > 0)
        {
            carry.Append(buf, 0, read);
        }

        var text = carry.ToString();
        int start = 0;
        while (true)
        {
            int nl = text.IndexOf('\n', start);
            if (nl < 0) break;
            int len = nl - start;
            if (len > 0 && text[start + len - 1] == '\r') len--;
            ProcessLine(text.Substring(start, len));
            start = nl + 1;
        }

        if (start > 0) carry.Remove(0, start);
    }

    private static void ProcessLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0)
        {
            Console.WriteLine();
            return;
        }

        var pt = HTCipher.TryDecryptBase64Line(trimmed, out _);
        if (pt is null)
        {
            WriteColored(line);
            return;
        }

        var expanded = pt.Replace(HTCipher.SplitToken, "\n").TrimEnd('\r', '\n');
        if (expanded.Length == 0)
        {
            Console.WriteLine();
            return;
        }
        foreach (var sub in expanded.Split('\n'))
        {
            WriteColored(sub.TrimEnd('\r'));
        }
    }

    private static readonly Regex SeverityRx = new(@":\s*(Fatal|Error|Warning|Display|Verbose|VeryVerbose)\s*:", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly object ConsoleLock = new();

    private static void WriteColored(string line)
    {
        ConsoleColor? fg = null;
        var m = SeverityRx.Match(line);
        if (m.Success)
        {
            fg = m.Groups[1].Value switch
            {
                "Fatal" => ConsoleColor.Magenta,
                "Error" => ConsoleColor.Red,
                "Warning" => ConsoleColor.Yellow,
                "Display" => ConsoleColor.White,
                "Verbose" => ConsoleColor.DarkGray,
                "VeryVerbose" => ConsoleColor.DarkGray,
                _ => null,
            };
        }

        lock (ConsoleLock)
        {
            if (fg.HasValue)
            {
                var prev = Console.ForegroundColor;
                Console.ForegroundColor = fg.Value;
                Console.WriteLine(line);
                Console.ForegroundColor = prev;
            }
            else
            {
                Console.WriteLine(line);
            }
        }
    }

    private static void PrintHeader(string path)
    {
        lock (ConsoleLock)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("=== HTLogTail -- NTE live log tail ===");
            Console.WriteLine($"File   : {path}");
            Console.WriteLine("Hint   : press Ctrl+C to exit");
            Console.WriteLine(new string('-', 80));
            Console.ForegroundColor = prev;
        }
    }

    private static void NotifyRotation()
    {
        lock (ConsoleLock)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine("--- log rotated, reopening ---");
            Console.ForegroundColor = prev;
        }
    }
}
