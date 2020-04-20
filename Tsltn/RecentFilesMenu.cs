using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Reflection;
using System.Globalization;

namespace Tsltn
{
    /// <summary>
    /// Klasse, die der Anwendung ein RecentFiles-Menü hinzufügt.
    /// </summary>
    /// <remarks>
    /// <para>Der Namespace dieser Datei muss der Root-Namespace der Anwendung sein.</para>
    /// 
    /// 
    /// <para>Um einen Dateinamen zu Properties.Settings.Default.RecentFiles hinzuzufügen muss RecentFilesMenu.AddRecentFile(string)
    /// aufgerufen werden. Das sollte immer nach dem Öffnen einer Datei geschehen (z.B. in einer Property "CurrentFileName").</para>
    /// 
    /// <para>Um eine Datei zu öffnen, muss das Event "RecentFileSelected" abonniert werden. Der Dateiname wird in den 
    /// RecentFileSelectedEventArgs geliefert.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
    internal class RecentFilesMenu : IRecentFilesMenu
    {
        private static class RecentFilesPersistence
        {
            private static readonly string _fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, $"{Environment.MachineName}.{Environment.UserName}.RF.txt");


            public static List<string> RecentFiles { get; } = new List<string>();

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
            public static Task LoadAsync()
            {
                return Task.Run(() =>
                {
                    if (File.Exists(_fileName))
                    {
                        string[] arr;
                        try
                        {
                            arr = File.ReadAllLines(_fileName);
                        }
                        catch
                        {
                            return;
                        }

                        lock (RecentFiles)
                        {
                            RecentFiles.Clear();
                            RecentFiles.AddRange(arr);
                        }
                    }
                });
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
            public static Task SaveAsync()
            {
                return Task.Run(() =>
                {
                    try
                    {
                        lock (RecentFiles)
                        {
                            File.WriteAllLines(_fileName, RecentFiles);
                        }
                    }
                    catch
                    {

                    }
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        const int MAX_DISPLAYED_FILE_PATH_LENGTH = 60;

        public event EventHandler<RecentFileSelectedEventArgs>? RecentFileSelected;


        #region private fields

        readonly ICommand _openRecentFileCommand;
        readonly ICommand _clearRecentFilesCommand;
        MenuItem? _miRecentFiles;

        #endregion


        #region ctor

        /// <summary>
        /// Initialisiert ein neues RecentFilesMenu.
        /// </summary>
        public RecentFilesMenu()
        {
            _openRecentFileCommand = new OpenRecentFile(new Action<object>(OpenRecentFile_Executed));
            _clearRecentFilesCommand = new ClearRecentFiles(new Action(ClearRecentFiles_Executed));
        }

        #endregion


        #region public Methods

        /// <summary>
        /// Weist dem <see cref="RecentFilesMenu"/> das <see cref="MenuItem"/> zu, als dessen Child das
        /// <see cref="RecentFilesMenu"/> angezeigt wird. Diese Methode muss vor allen anderen aufgerufen werden!
        /// </summary>
        /// <param name="miRecentFiles">Das <see cref="MenuItem"/> zu, als dessen Child das
        /// <see cref="RecentFilesMenu"/> angezeigt wird.</param>
        /// <exception cref="ArgumentNullException"><paramref name="miRecentFiles"/> ist <c>null</c>.</exception>
        public void SetRecentFilesMenuItem(MenuItem miRecentFiles)
        {
            if (miRecentFiles is null)
            {
                throw new ArgumentNullException(nameof(miRecentFiles));
            }

            _miRecentFiles = miRecentFiles;
            _miRecentFiles.Loaded += miRecentFiles_Loaded;
        }

        /// <summary>
        /// Fügt <paramref name="fileName"/> zu Properties.Settings.Default.RecentFiles hinzu, wenn 
        /// <paramref name="fileName"/> einen Dateinamen enthält und speichert die Settings.
        /// </summary>
        /// <param name="fileName">Ein hinzuzufügender Dateiname. Wenn <paramref name="fileName"/> null, 
        /// leer oder Whitespace ist, wird nichts hinzugefügt.</param>
        public Task AddRecentFileAsync(string fileName)
        {
            if (_miRecentFiles is null)
            {
                throw new InvalidOperationException($"The MenuItem has not been initialized. Call {nameof(SetRecentFilesMenuItem)} first!");
            }

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                lock (RecentFilesPersistence.RecentFiles)
                {
                    RecentFilesPersistence.RecentFiles.Remove(fileName);
                    RecentFilesPersistence.RecentFiles.Insert(0, fileName);

                    if (RecentFilesPersistence.RecentFiles.Count > 10)
                    {
                        RecentFilesPersistence.RecentFiles.RemoveAt(10);
                    }
                }

                return RecentFilesPersistence.SaveAsync();
            }

            return Task.CompletedTask;
        }



        /// <summary>
        /// Enfernt einen Dateinamen aus der Liste.
        /// </summary>
        /// <param name="fileName">Der Dateiname.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public Task RemoveRecentFileAsync(string fileName)
        {
            if (_miRecentFiles is null)
            {
                throw new InvalidOperationException($"The MenuItem has not been initialized. Call {nameof(SetRecentFilesMenuItem)} first!");
            }

            lock (RecentFilesPersistence.RecentFiles)
            {
                RecentFilesPersistence.RecentFiles.Remove(fileName);
            }

            return RecentFilesPersistence.SaveAsync();
        }


        /// <summary>
        /// Gibt den Namen der zuletzt geöffneten Datei zurück oder null, wenn dieser nicht existiert.
        /// </summary>
        /// <returns>Name der zuletzt geöffneten Datei zurück oder null, wenn dieser nicht existiert.</returns>
        public string? GetMostRecentFile()
        {
            lock (RecentFilesPersistence.RecentFiles)
            {
                return RecentFilesPersistence.RecentFiles.FirstOrDefault();
            }
        }

        #endregion


        #region private


        #region miRecentFiles_Loaded

        private void miRecentFiles_Loaded(object sender, RoutedEventArgs e)
        {
            if (_miRecentFiles is null)
            {
                throw new InvalidOperationException($"The MenuItem has not been initialized. Call {nameof(SetRecentFilesMenuItem)} first!");
            }

            _miRecentFiles.Dispatcher.BeginInvoke(new Action(
                async () =>
                {
                    await RecentFilesPersistence.LoadAsync().ConfigureAwait(true);
                    UpdateRecentFiles();
                }), DispatcherPriority.Send);
        }


        private void UpdateRecentFiles()
        {
            if (_miRecentFiles is null)
            {
                throw new InvalidOperationException($"The MenuItem has not been initialized. Call {nameof(SetRecentFilesMenuItem)} first!");
            }

            _miRecentFiles.Items.Clear();

            if (RecentFilesPersistence.RecentFiles.Count == 0)
            {
                _miRecentFiles.IsEnabled = false;
            }
            else
            {
                try
                {
                    _miRecentFiles.IsEnabled = true;

                    var recentFiles = RecentFilesPersistence.RecentFiles;

                    lock (recentFiles)
                    {
                        for (int i = 0; i < recentFiles.Count; i++)
                        {
                            if (recentFiles[i] == null) continue;

                            var mi = new MenuItem
                            {
                                Header = GetMenuItemHeaderFromFilename(recentFiles[i], i),
                                Command = _openRecentFileCommand,
                                CommandParameter = recentFiles[i]
                            };

                            _miRecentFiles.Items.Add(mi);
                        }
                    }

                    var menuItemClearList = new MenuItem
                    {
                        Header = "Liste _leeren",
                        Command = _clearRecentFilesCommand
                    };

                    _miRecentFiles.Items.Add(new Separator());
                    _miRecentFiles.Items.Add(menuItemClearList);
                }
                catch
                {
                    _miRecentFiles.Items.Clear();
                    _miRecentFiles.IsEnabled = false;

                    lock (RecentFilesPersistence.RecentFiles)
                    {
                        RecentFilesPersistence.RecentFiles.Clear();
                    }
                    RecentFilesPersistence.SaveAsync();
                }
            }
        }


        private static string GetMenuItemHeaderFromFilename(string fileName, int i)
        {
            if (fileName.Length > MAX_DISPLAYED_FILE_PATH_LENGTH)
            {
                int fileNameLength = Path.GetFileName(fileName).Length + 1;
                int restLength = MAX_DISPLAYED_FILE_PATH_LENGTH - fileNameLength - 3;
                fileName = restLength >= 0 ?
                    fileName.Substring(0, restLength) + "..." + fileName.Substring(fileName.Length - fileNameLength) :
                    fileName;
            }

            if (i < 9)
            {
                fileName = "_" + (i + 1).ToString(CultureInfo.InvariantCulture) + ": " + fileName;
            }
            else if (i == 9)
            {
                fileName = "1_0: " + fileName;
            }

            return fileName;
        }

        #endregion


        #region Command-Execute-Handler

        private void OpenRecentFile_Executed(object fileName)
        {
            OnRecentFileSelected((string)fileName);
        }


        private void ClearRecentFiles_Executed()
        {
            if (_miRecentFiles is null)
            {
                throw new InvalidOperationException($"The MenuItem has not been initialized. Call {nameof(SetRecentFilesMenuItem)} first!");
            }

            lock (RecentFilesPersistence.RecentFiles)
            {
                RecentFilesPersistence.RecentFiles.Clear();
            }
            RecentFilesPersistence.SaveAsync();
        }

        #endregion


        private void OnRecentFileSelected(string fileName)
        {
            RecentFileSelected?.Invoke(this, new RecentFileSelectedEventArgs(fileName));
        }

        #endregion

    }//class


    ////////////////////////////////////////////////////////////////////////////////////////////////////////





    public class OpenRecentFile : ICommand
    {
        private readonly Action<object> _executeHandler;

        public OpenRecentFile(Action<object> execute)
        {
            _executeHandler = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler? CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeHandler(parameter);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////


    public class ClearRecentFiles : ICommand
    {
        private readonly Action _executeHandler;

        public ClearRecentFiles(Action execute)
        {
            _executeHandler = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler? CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeHandler();
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////


    public class RecentFileSelectedEventArgs : EventArgs
    {
        public RecentFileSelectedEventArgs(string fileName)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Der zum Öffnen ausgewählte Dateiname.
        /// </summary>
        public string FileName { get; private set; }
    }

}
