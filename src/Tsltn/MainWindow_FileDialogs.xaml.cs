using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;
using Tsltn.Resources;

namespace Tsltn;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow
{
    private bool GetTsltnInFileName(out string tsltnFileName)
    {
        var dlg = new OpenFileDialog()
        {
            AddExtension = true,
            CheckFileExists = true,
            CheckPathExists = true,

            DefaultExt = App.TsltnFileExtension,
            Filter = $"{Res.TsltnFile} (*{App.TsltnFileExtension})|*{App.TsltnFileExtension}",
            DereferenceLinks = true,
            InitialDirectory = Environment.CurrentDirectory,
            Multiselect = false,
            ValidateNames = true
        };

        if (dlg.ShowDialog(this) == true)
        {
            tsltnFileName = dlg.FileName;
            return true;
        }

        tsltnFileName = "";
        return false;
    }

    private bool GetTsltnOutFileName([NotNullWhen(true)] ref string? fileName)
    {
        string? directory = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(directory))
        {
            directory = Environment.CurrentDirectory;
        }

        var dlg = new SaveFileDialog()
        {
            FileName = Path.GetFileName(fileName) ?? "",
            AddExtension = true,
            CheckFileExists = false,
            CheckPathExists = true,
            CreatePrompt = false,
            Filter = $"{Res.TsltnFile} (*{App.TsltnFileExtension})|*{App.TsltnFileExtension}",
            InitialDirectory = directory,
            DefaultExt = App.TsltnFileExtension,
            DereferenceLinks = true
        };

        if (dlg.ShowDialog(this) == true)
        {
            fileName = dlg.FileName;
            return true;
        }

        return false;
    }

    private bool GetXmlInFileName([NotNullWhen(true)] ref string? sourcefilePath)
    {
        string? directory = Path.GetDirectoryName(sourcefilePath);
        if (string.IsNullOrEmpty(directory))
        {
            directory = Environment.CurrentDirectory;
        }

        var dlg = new OpenFileDialog()
        {
            FileName = Path.GetFileName(sourcefilePath) ?? "",
            AddExtension = true,
            CheckFileExists = true,
            CheckPathExists = true,

            DefaultExt = ".xml",
            Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
            DereferenceLinks = true,
            InitialDirectory = directory,
            Multiselect = false,
            ValidateNames = true,
            Title = Res.LoadXmlFile
        };

        if (dlg.ShowDialog(this) == true)
        {
            sourcefilePath = dlg.FileName;
            return true;
        }

        return false;
    }


    private bool GetXmlOutFileName([NotNullWhen(true)] ref string? fileName)
    {
        string? directory = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(directory))
        {
            directory = Environment.CurrentDirectory;
        }

        var dlg = new SaveFileDialog()
        {
            Title = Res.SaveTranslationAs,
            FileName = Path.GetFileName(fileName) ?? "",
            AddExtension = true,
            CheckFileExists = false,
            CheckPathExists = true,
            CreatePrompt = false,
            Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
            InitialDirectory = directory,
            DefaultExt = ".xml",
            DereferenceLinks = true
        };

        if (dlg.ShowDialog(this) == true)
        {
            fileName = dlg.FileName;
            return true;
        }

        return false;
    }
}
