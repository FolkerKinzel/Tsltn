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

/// <summary>
/// Translates the xml comments of the *.cs files in a specified
/// directory and all its sub-directories using a specified TSLTN 
/// file.
/// </summary>
/// <remarks>
/// <para>
/// Translating the source files is an irreversible and risky operation.
/// DON'T USE THIS PROGRAM!
/// </para>
/// <para>
/// If you ignore my advice do the following instead:
/// </para>
/// <list type="number">
/// <item>Specify an output path that is different from the input path
/// using the option --out.</item>
/// <item>Set the -t option to run the program in test mode.</item>
/// <item>Inspect the console output and the Reverser.log file to
/// see which items could not be translated.</item>
/// <item>Provide a replacement file containing the types and members
/// of the untranslated items and provide it using the option -r. Look 
/// at VCardsReplacements.csv as an example. </item>
/// <item>Another pitfall that leads to untranslated items is that *.cs 
/// files containing extended characters in the comments had been saved 
/// in an ANSI encoding. Such entries show question marks instead of letters
/// in the console. Reopen these files in Visual Studio and save it as UTF-8.</item>
/// <item>Once you've done all that, remove the -t option from the command line
/// and run the program to get a preview of the result in your specified --out
/// directory.</item>
/// <item>If you are satisfied with the test, make shure to have committed all
/// changes to your project. Then remove the --out option and run the program.</item>
/// </list>
/// 
/// </remarks>
internal sealed partial class Reverser : IReverser
{
    private readonly ILogger _log;
    private readonly IMessage _msg;
    private readonly StringBuilder _builder = new();
    private static readonly XElement _emptyCodeBlock = new("code");


    private const string SANDCASTLE_CODE = "code";
    private const string LINE_START = "    /// ";
    private const int LINE_LENGTH = 75;
    private const string CREF_NORMALIZED = "cref=\"";

    [GeneratedRegex("cref\\s*=\\s*\"", RegexOptions.CultureInvariant, 50)]
    private partial Regex SpaceNormalizerRegex();

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

        if (arguments.ReplacementsPath != null)
        {
            ParseReplacements(arguments.ReplacementsPath);
        }

        _log.Debug("Reverser initialized.");
    }

    public string TsltnPath { get; }
    public string InPath { get; }
    public string OutPath { get; }
    public bool TestRun { get; }
    public Dictionary<int, string> Translations { get; } = new Dictionary<int, string>();

    public List<(string, string)> Replacements { get; } =
        new()
        {
            ("cref=\"object.ToString", "cref=\"M:System.Object.ToString"),

            ("cref=\"Stream\"", "cref=\"T:System.IO.Stream\""),
            ("cref=\"Stream.Position", "cref=\"P:System.IO.Stream.Position"),

            ("cref=\"int\"", "cref=\"T:System.Int32\""),
            ("cref=\"long\"", "cref=\"T:System.Int64\""),
            ("cref=\"char\"", "cref=\"T:System.Char\""),
            ("cref=\"char.IsWhiteSpace(char)", "cref=\"M:System.Char.IsWhiteSpace(System.Char)"),
            ("cref=\"byte\"", "cref=\"T:System.Byte\""),
            ("cref=\"string\"", "cref=\"T:System.String\""),
            ("cref=\"string.Empty", "cref=\"F:System.String.Empty"),
            ("cref=\"string.GetHashCode()", "cref=\"M:System.String.GetHashCode"),
            ("cref=\"StringComparison\"", "cref=\"T:System.StringComparison\""),
            ("cref=\"ArgumentException\"", "cref=\"T:System.ArgumentException\""),
            ("cref=\"NullReferenceException\"", "cref=\"T:System.NullReferenceException\""),
            ("cref=\"MD5\"", "cref=\"T:System.Security.Cryptography.MD5\""),
            ("cref=\"SHA256\"", "cref=\"T:System.Security.Cryptography.SHA256\""),
            ("cref=\"Uri\"", "cref=\"T:System.Uri\""),
            ("cref=\"bool\"", "cref=\"T:System.Boolean\""),

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

            ("cref=\"Environment.NewLine", "cref=\"P:System.Environment.NewLine"),
            ("cref=\"Environment.MachineName", "cref=\"P:System.Environment.MachineName"),
            ("cref=\"Environment.UserName", "cref=\"P:System.Environment.UserName"),

            ("cref=\"Base64FormattingOptions\"", "cref=\"T:System.Base64FormattingOptions\""),
            ("cref=\"Base64FormattingOptions.None", "cref=\"F:System.Base64FormattingOptions.None"),

            ("cref=\"TextReader\"", "cref=\"T:System.IO.TextReader\""),

            ("cref=\"XElement\"", "cref=\"T:System.Xml.Linq.XElement\""),

            ("cref=\"Action\"", "cref=\"T:System.Action\""),
            ("cref=\"Action{T}\"", "cref=\"T:System.Action`1\""),
            ("cref=\"Task\"", "cref=\"T:System.Threading.Tasks.Task\""),
            ("cref=\"Task.WhenAll(IEnumerable{Task})", "cref=\"M:System.Threading.Tasks.Task.WhenAll(System.Collections.Generic.IEnumerable{System.Threading.Tasks.Task})"),
            ("cref=\"Mutex\"", "cref=\"T:System.Threading.Mutex\""),

            ("cref=\"Path.GetInvalidPathChars", "cref=\"M:System.IO.Path.GetInvalidPathChars"),
            ("cref=\"EventArgs\"", "cref=\"T:System.EventArgs\""),

            ("cref=\"IEnumerable{T}\"", "cref=\"T:System.Collections.Generic.IEnumerable`1\""),
            ("cref=\"IEnumerable\"", "cref=\"T:System.Collections.IEnumerable\""),
            ("cref=\"List{T}\"", "cref=\"T:System.Collections.Generic.List`1\""),

            ("cref=\"CultureInfo\"", "cref=\"T:System.Globalization.CultureInfo\"")
        };



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
            string value = xElement.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            int hash = (int)long.Parse(xElement.Attribute("ID")!.Value, NumberStyles.AllowHexSpecifier);
            Translations[hash] = value;
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

        list.AddRange(Directory.EnumerateFiles(directory, "*.cs")
                               .Where((f) => !(f.EndsWith(".Designer.cs") || f.EndsWith(".g.cs"))));

        list.TrimExcess();
        return list;
    }

    private void ParseReplacements(string replacementsPath)
    {
        _log.Debug("Start parsing replacements.");

        var lines = File.ReadAllLines(replacementsPath);

        foreach (string line in lines)
        {
            ReadOnlySpan<char> span = line.AsSpan();
            if (span.IsWhiteSpace() || span.TrimStart().StartsWith('#'))
            {
                continue;
            }

            _log.Debug("Parse: {0}", line);
            int splitPoint = GetSplitPoint(line);

            if (splitPoint == -1)
            {
                throw new FormatException();
            }

            Replacements.Add((line.AsSpan(0, splitPoint).Trim().ToString(), 
                              line.AsSpan(splitPoint + 1).Trim().ToString()));
        }

        _log.Debug("Replacements successfully parsed.");

        static int GetSplitPoint(string line)
        {
            ReadOnlySpan<char> span = line.AsSpan();

            int bracesCounter = 0;

            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];

                if (c is ')' or '}')
                {
                    bracesCounter--;
                    continue;
                }
                else if (c is '(' or '{')
                {
                    bracesCounter++;
                    continue;
                }

                if (bracesCounter == 0 && c == ',')
                {
                    return i;
                }
            }

            return -1;
        }
    }

    private void TranslateFile(string file)
    {
        const string rootStart = "<R>";
        const string rootEnd = "</R>";

        //if (file.EndsWith("\\SpanExtension_LastIndexOf.cs"))
        //{

        //}
        _log.Information("===================================================");
        _log.Information("");
        _log.Information("Start translating {0}", file);

        var lines = File.ReadAllLines(file).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (IsCommentsLine(line) && line.Contains('<', StringComparison.Ordinal))
            {
                _builder.Clear()
                        .Append(rootStart)
                        .Append(StripCommentsLine(line));
                lines.RemoveAt(i);

                // Don't use the 'line' variable here!:
                while (i < lines.Count && IsCommentsLine(lines[i]))
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

        if (!TestRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            File.WriteAllLines(outPath, lines);
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

            if (Translations.TryGetValue(Hash(toTranslate),
                                         out string? translation))
            {
                //if (translation.Contains("cref=\"P:System.Span`1.Empty\""))
                //{

                //}

                translation = SpaceNormalizerRegex().Replace(translation, CREF_NORMALIZED);

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

                    int end = Math.Min(childCodeNodes.Length, tmpCodeNodes.Length);

                    for (int i = 0; i < end; i++)
                    {
                        tmpCodeNodes[i].ReplaceWith(childCodeNodes[i]);
                    }

                    child.ReplaceNodes(tmp.Nodes());
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
        } //foreach

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

            if (idx < 0)
            {
                builder.Append(LINE_START).AppendLine(span);
                return ReadOnlySpan<char>.Empty;
            }
            idx += LINE_LENGTH;
            builder.Append(LINE_START).AppendLine(span.Slice(0, idx));
            return span.Slice(idx + 1);
        }
    }


    private int Hash(string toTranslate)
    {
        //Match match = NormalizerRegex().Match(toTranslate);
        //if (match.Success)
        //{
        //    toTranslate = toTranslate.Replace(match.Value,
        //                                      match.Value.Substring(0, match.Length - 2),
        //                                      StringComparison.Ordinal);
        //    return Hash(toTranslate);
        //}

        toTranslate = SpaceNormalizerRegex().Replace(toTranslate, CREF_NORMALIZED);

        foreach ((string Original, string Replacement) item in Replacements)
        {
            toTranslate = toTranslate.Replace(item.Original,
                                              item.Replacement,
                                              StringComparison.Ordinal);
        }

        var tmp = XElement.Parse(string.Concat("<R>", toTranslate, "</R>"), LoadOptions.None);
        XElement[] tmpCodeNodes = tmp.Elements(SANDCASTLE_CODE).ToArray();

        if (tmpCodeNodes.Length > 0)
        {
            for (int i = 0; i < tmpCodeNodes.Length; i++)
            {
                tmpCodeNodes[i].ReplaceWith(_emptyCodeBlock);
            }
        }

        toTranslate = tmp.InnerXml();

        return toTranslate.GetPersistentHashCode(HashType.AlphaNumericIgnoreCase);
    }
}



