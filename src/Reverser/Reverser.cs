// See https://aka.ms/new-console-template for more information

using FolkerKinzel.Strings;
using FolkerKinzel.XmlFragments;
using Serilog;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;

namespace Reversers;

internal sealed partial class Reverser : IReverser
{
    private readonly ILogger _log;
    private readonly IMessage _msg;
    private readonly StringBuilder _builder = new();

    private const string SANDCASTLE_CODE = "code";
    private const string LINE_START = "    /// ";
    private const int LINE_LENGTH = 80;

    [GeneratedRegex("cref\\s*=\\s*\".*?\\(\\)", RegexOptions.CultureInvariant, 50)]
    private partial Regex NormalizerRegex();

    public Reverser(CommandLineArgument arguments, ILogger logger, IMessage msg)
    {
        this._log = logger;
        this._msg = msg;


        TsltnPath = Path.GetFullPath(arguments.TsltnFile);
        InPath = Path.GetFullPath(arguments.InputPath);
        InPath = Path.GetDirectoryName(InPath) ?? InPath;

        if (arguments.OutputPath is null)
        {
            OutPath = InPath;
        }
        else
        {
            OutPath = Path.GetFullPath(arguments.OutputPath);
            OutPath = Path.GetDirectoryName(OutPath) ?? OutPath;
        }

        TestRun = arguments.TestRun;

        _log.Debug("Reverser initialized.");
    }

    public string TsltnPath { get; }
    public string InPath { get; }
    public string OutPath { get; }
    public bool TestRun { get; }

    public (string, string)[] Replacements { get; } =
        new (string, string)[]
        {
            ("cref=\"Stream\"", "cref=\"T:System.IO.Stream\""),
            ("cref=\"Stream.Position", "cref=\"P:System.IO.Stream.Position"),

            ("cref=\"int\"", "cref=\"T:System.Int32\""),
            ("cref=\"long\"", "cref=\"T:System.Int64\""),
            ("cref=\"char\"", "cref=\"T:System.Char\""),
            ("cref=\"byte\"", "cref=\"T:System.Byte\""),

            ("cref=\"string\"", "cref=\"T:System.String\""),

            ("cref=\"string.Empty", "cref=\"F:System.String.Empty"),
            ("cref=\"string.GetHashCode", "cref=\"M:System.String.GetHashCode"),
            ("cref=\"StringComparison\"", "cref=\"T:System.StringComparison\""),
            ("cref=\"ArgumentException\"", "cref=\"T:System.ArgumentException\""),
            ("cref=\"NullReferenceException\"", "cref=\"T:System.NullReferenceException\""),
            ("cref=\"MD5\"", "cref=\"T:System.Security.Cryptography.MD5\""),
            ("cref=\"SHA256\"", "cref=\"T:System.Security.Cryptography.SHA256\""),

            ("cref=\"byte\"", "cref=\"T:System.Byte\""),
            ("cref=\"StringBuilder\"", "cref=\"T:System.Text.StringBuilder\""),
            ("cref=\"StringBuilder.MaxCapacity", "cref=\"P:System.Text.StringBuilder.MaxCapacity"),

            ("cref=\"Encoding\"", "cref=\"T:System.Text.Encoding\""),
            ("cref=\"Encoding.EncoderFallback", "cref=\"P:System.Text.Encoding.EncoderFallback"),
            ("cref=\"Encoding.DecoderFallback", "cref=\"P:System.Text.Encoding.DecoderFallback"),
            ("cref=\"Encoding.UTF8", "cref=\"P:System.Text.Encoding.UTF8"),
            ("cref=\"Encoding.WebName", "cref=\"P:System.Text.Encoding.WebName"),


            ("cref=\"Span{T}.Empty", "cref=\"P:System.Span`1.Empty"),
            ("cref=\"Span{T}\"", "cref=\"T:System.Span`1\""),
            ("cref=\"ReadOnlySpan{T}\"", "cref=\"T:System.ReadOnlySpan`1\""),
            ("cref=\"ReadOnlySpan{T}.Empty", "cref=\"P:System.ReadOnlySpan`1.Empty"),
            ("cref=\"ReadOnlyMemory{T}\"", "cref=\"T:System.ReadOnlyMemory`1\""),
            ("cref=\"ReadOnlySpanExtension\"", "cref=\"T:FolkerKinzel.Strings.ReadOnlySpanExtension\""),
            ("cref=\"MemoryExtensions.IndexOfAny{T}(ReadOnlySpan{T}, ReadOnlySpan{T})", "cref=\"M:System.MemoryExtensions.IndexOfAny``1(System.ReadOnlySpan{``0},System.ReadOnlySpan{``0})"),
            ("cref=\"string.IndexOfAny(char[])", "cref=\"M:System.String.IndexOfAny(System.Char[])"),
            ("cref=\"MemoryExtensions.LastIndexOfAny{T}(ReadOnlySpan{T}, ReadOnlySpan{T})", "cref=\"M:System.MemoryExtensions.LastIndexOfAny``1(System.ReadOnlySpan{``0},System.ReadOnlySpan{``0})"),
            ("cref=\"string.LastIndexOfAny(char[])", "cref=\"M:System.String.LastIndexOfAny(System.Char[])"),
            ("cref=\"Environment.NewLine", "cref=\"P:System.Environment.NewLine"),

            ("cref=\"Base64FormattingOptions\"", "cref=\"T:System.Base64FormattingOptions\""),
            ("cref=\"Base64FormattingOptions.None", "cref=\"F:System.Base64FormattingOptions.None"),

            ("cref=\"DecoderValidationFallback\"", "cref=\"T:FolkerKinzel.Strings.DecoderValidationFallback\""),
            ("cref=\"DecoderValidationFallback.HasError", "cref=\"P:FolkerKinzel.Strings.DecoderValidationFallback.HasError"),

            ("cref=\"CharExtension.IsNewLine(char)", "cref=\"M:FolkerKinzel.Strings.CharExtension.IsNewLine(System.Char)"),

            ("cref=\"VCard\"", "cref=\"T:FolkerKinzel.VCards.VCard\""),
            ("cref=\"PropertyClassTypes\"", "cref=\"T:FolkerKinzel.VCards.Models.Enums.PropertyClassTypes\""),
            ("cref=\"ImppTypes\"", "cref=\"T:FolkerKinzel.VCards.Models.Enums.ImppTypes\""),
            ("cref=\"AddressTypes\"", "cref=\"T:FolkerKinzel.VCards.Models.Enums.AddressTypes\""),
            ("cref=\"RelationTypes\"", "cref=\"T:FolkerKinzel.VCards.Models.Enums.RelationTypes\""),
            ("cref=\"ITimeZoneIDConverter\"", "cref=\"T:FolkerKinzel.VCards.Models.ITimeZoneIDConverter\""),

            ("cref=\"IEnumerable{T}\"", "cref=\"T:System.Collections.Generic.IEnumerable`1\""),
            ("cref=\"TextReader\"", "cref=\"T:System.IO.TextReader\""),
            ("cref=\"VCardProperty\"", "cref=\"T:FolkerKinzel.VCards.Models.VCardProperty\""),

        };

    public Dictionary<int, string> Translations { get; } = new Dictionary<int, string>();


    public void Run()
    {
        _log.Debug("Start initializing translations.");
        InitTranslations();
        _log.Debug("Translations successfully initialized.");

        if (!_msg.ShouldRun(this))
        {
            return;
        }

        _log.Debug("Start initializing source files.");
        List<string> files = InitFiles(InPath, new List<string>());
        _log.Debug("Source files successfully initialized.");

        foreach (string file in files)
        {
            try
            {
                TranslateFile(file);
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }


    private void InitTranslations()
    {
        XElement root = XDocument.Parse(File.ReadAllText(TsltnPath), LoadOptions.None).Root!;

        foreach (XElement xElement in root.Elements())
        {
            int hash = (int)long.Parse(xElement.Attribute("ID")!.Value, NumberStyles.AllowHexSpecifier);
            Translations[hash] = xElement.Value;
        }

        Translations.TrimExcess();
    }


    private List<string> InitFiles(string directory, List<string> list)
    {
        foreach (string subDirectory in Directory.EnumerateDirectories(directory))
        {
            if (subDirectory.EndsWith("obj", StringComparison.Ordinal) ||
                subDirectory.EndsWith("bin", StringComparison.Ordinal))
            {
                continue;
            }
            InitFiles(subDirectory, list);
        }

        list.AddRange(Directory.EnumerateFiles(directory, "*.cs"));

        list.TrimExcess();
        return list;
    }

    private void TranslateFile(string file)
    {
        const string rootStart = "<R>";
        const string rootEnd = "</R>";

        _log.Information("Start translating {0}", file);

        var lines = File.ReadAllLines(file).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            if (IsCommentsLine(lines[i]))
            {
                _builder.Clear()
                        .Append(rootStart)
                        .Append(StripCommentsLine(lines[i]));
                lines.RemoveAt(i);

                while (IsCommentsLine(lines[i]) && i < lines.Count)
                {
                    _builder.Append(' ').Append(StripCommentsLine(lines[i]));
                    lines.RemoveAt(i);
                }
                _builder.Append(rootEnd);

                try
                {
                    lines.Insert(i, TranslateComment(_builder.ToString()));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, ex.Message);
                }

            }
        }

        string relative = Path.GetRelativePath(InPath, file);
        string outPath = Path.Combine(OutPath, relative);
        _log.Information("Save\n{0} to\n{1}.", file, outPath);

        //string test = string.Join(Environment.NewLine, lines);

        if (!TestRun)
        {
            //Directory.CreateDirectory(outPath);
            //File.WriteAllLines(outPath, lines);
        }

        static bool IsCommentsLine(ReadOnlySpan<char> line)
            => line.TrimStart().StartsWith("///", StringComparison.Ordinal);

        static ReadOnlySpan<char> StripCommentsLine(ReadOnlySpan<char> line)
            => line.TrimStart().Slice(3).Trim();
    }


    private string TranslateComment(string xml)
    {
        var root = XElement.Parse(_builder.ToString(), LoadOptions.None);

        foreach (XElement child in root.Elements())
        {
            string toTranslate = child.InnerXml();
            _log.Information("Try to translate: {0}", toTranslate);
            toTranslate = Normalize(toTranslate);

            if (Translations.TryGetValue(toTranslate.GetPersistentHashCode(HashType.AlphaNumericIgnoreCase),
                                        out string? translation))
            {
                foreach ((string Original, string Replacement) item in Replacements)
                {
                    translation = translation.Replace(item.Replacement,
                                                      item.Original,
                                                      StringComparison.Ordinal);
                }

                _log.Information("Translation found: {0}", translation);

                try
                {
                    var tmp = XElement.Parse(string.Concat("<R>", translation, "</R>"), LoadOptions.None);

                    XElement[] childCodeNodes = child.Elements(SANDCASTLE_CODE).ToArray();
                    XElement[] tmpCodeNodes = tmp.Elements(SANDCASTLE_CODE).ToArray();

                    child.ReplaceNodes(tmp.Nodes());

                    int end = Math.Min(childCodeNodes.Length, tmpCodeNodes.Length);

                    for (int i = 0; i < end; i++)
                    {
                        tmpCodeNodes[i].ReplaceWith(childCodeNodes[i]);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, ex.Message);
                    continue;
                }
            }
            else
            {
                _log.Warning("No translation found.");
                continue;
            }
        }

        _builder.Clear();
        foreach (XElement child in root.Elements())
        {
            string s = XmlFragmentBeautifier.Beautify(child.ToString());

            foreach (string line in s.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                ReadOnlySpan<char> span = line.AsSpan();

                while (span.Length > LINE_LENGTH)
                {
                    span = Wrap(span, _builder);
                }

                if (span.Length != 0)
                {
                    _builder.Append(LINE_START).AppendLine(span);
                }
            }
        }

        return _builder.TrimEnd().ToString();

        //////////////////////////////////////////////

        static ReadOnlySpan<char> Wrap(ReadOnlySpan<char> span, StringBuilder builder)
        {
            int idx = span.Slice(LINE_LENGTH).IndexOf(' ');

            if(idx < 0)
            {
                builder.Append(LINE_START).AppendLine(span);
                return ReadOnlySpan<char>.Empty;
            }
            idx += LINE_LENGTH;
            builder.Append(LINE_START).AppendLine(span.Slice(0, idx));
            return span.Slice(idx + 1);
        }
    }


    private string Normalize(string toTranslate)
    {
        Match match = NormalizerRegex().Match(toTranslate);
        if (match.Success)
        {
            toTranslate = toTranslate.Replace(match.Value,
                                              match.Value.Substring(0, match.Length - 2),
                                              StringComparison.Ordinal);
            return Normalize(toTranslate);
        }


        foreach ((string Original, string Replacement) item in Replacements)
        {
            toTranslate = toTranslate.Replace(item.Original,
                                              item.Replacement,
                                              StringComparison.Ordinal);
        }

        return toTranslate;
    }
}



