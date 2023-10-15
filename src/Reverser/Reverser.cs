// See https://aka.ms/new-console-template for more information

using FolkerKinzel.Strings;
using Serilog;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Reversers;

internal sealed class Reverser : IReverser
{
    private readonly ILogger _log;
    private readonly IMessage _msg;
    private readonly StringBuilder _builder = new StringBuilder();

    public Reverser(CommandLineArguments arguments, ILogger logger, IMessage msg)
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

        _log.Information("Start translating {0}.", file);

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
                    _builder.Append(StripCommentsLine(lines[i]));
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
            //Directory.CreateDirectory(outPath);
            //File.WriteAllLines(outPath, lines);
        }

        static bool IsCommentsLine(ReadOnlySpan<char> line) 
            => line.TrimStart().StartsWith("///", StringComparison.Ordinal);

        static ReadOnlySpan<char> StripCommentsLine(ReadOnlySpan<char> line)
            => line.TrimStart().Slice(3);

        
    }

    

    private string TranslateComment(string xml)
    {
        XElement root = XElement.Parse(_builder.ToString(), LoadOptions.None);

        foreach (XElement child in root.Elements())
        {
            string toTranslate = child.InnerXml();
            _log.Information("Try to translate {0}.", toTranslate);

            if(Translations.TryGetValue(toTranslate.GetPersistentHashCode(HashType.AlphaNumericIgnoreCase), 
                                        out string? translation))
            {
                _log.Information("Translation found: {0}", translation);
            }
            else
            {
                _log.Warning("No translation found.");
                continue;
            }
        }

        return "";
    }
}

