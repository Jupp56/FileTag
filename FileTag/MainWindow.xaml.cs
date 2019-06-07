using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FileTag
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string CurrentFolder;
        public string GetCurrentFolder()
        {
            return CurrentFolder;
        }
        public void SetCurrentFolder(string value)
        {

            this.CurrentFolder = value;
            this.CurrentDrive = Path.GetPathRoot(value);
        }
        string CurrentDrive;
        public static string MetaFile = ".FileTag";

        private ObservableCollection<string> ActiveFolders = new ObservableCollection<string>();
        private ObservableCollection<FSItem> ActiveFiles = new ObservableCollection<FSItem>();
        private ObservableCollection<FileT> AdditionalTag = new ObservableCollection<FileT>();
        private ObservableCollection<FileWithTagString> SearchResults = new ObservableCollection<FileWithTagString>();
        private List<FileWithTagString> FileTags = new List<FileWithTagString>();
        private int LastSelectedFileIndex = -1;
        private const int DataStructureVersion = 2;
        public MainWindow()
        {
            //#REMOVE
            SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            GetFolders();
            GetFiles();
            InitializeComponent();
            Folderbox.ItemsSource = ActiveFolders;
            Files.ItemsSource = ActiveFiles;
            AdditionalTags.ItemsSource = AdditionalTag;
            Search_Results.ItemsSource = SearchResults;
        }


        #region SaveAndLoad
        private void SaveState()
        {
            if (ActiveFiles.Count > 0)
            {
                UpdateFiles();
                UpdateFileTags();
                JSONHandler.WriteJSONInfo(CurrentDrive, MetaFile, CurrentFolder, FileTags, DataStructureVersion);
            }
        }

        private void GetFolders()
        {
            ActiveFolders.Clear();
            foreach (string s in Directory.GetDirectories(GetCurrentFolder()))
            {
                ActiveFolders.Add(s);
            }

        }

        private void GetFiles()
        {
            ActiveFiles.Clear();

            try
            {
                FileTags = JSONHandler.ReadJSONInfoFromDirectory(CurrentDrive, MetaFile, GetCurrentFolder(), DataStructureVersion);
            }
            catch(DataStructureVersionToHighException ex)
            {
                ShowUpdateRequired();
                return;
            }

            string[] Files = Directory.GetFiles(GetCurrentFolder());
            List<FileInfo> FileInfos = new List<FileInfo>();

            foreach (string file in Files)
            {
                FileInfos.Add(new FileInfo(Path.Combine(GetCurrentFolder(), file)));
            }

            foreach (FileInfo f in FileInfos)
            {
                ActiveFiles.Add(new FSItem(f.FullName, f.Length, f.LastWriteTime, LookupTags(f.FullName)));
            }
        }

        private void UpdateFiles()
        {
            if (ActiveFiles != null && ActiveFiles.Count > 0 && LastSelectedFileIndex >= 0 && AdditionalTag != null)
                ActiveFiles[LastSelectedFileIndex].SetTags(AdditionalTag.ToList());
        }

        private void UpdateFileTags()
        {
            try
            {
                FileTags.Clear();

                foreach (FSItem fsItem in ActiveFiles)
                {
                    if (fsItem.Tags.Count > 0) FileTags.Add(new FileWithTagString(fsItem.CompletePath, fsItem.Tags.ToList()));
                }

            }
            catch { }
        }

        #endregion

        private List<FileT> LookupTags(string filename)
        {
            try
            {
                return FileTags.Find(x => x.FullName == filename).Tags;
            }
            catch { return null; }
        }

        public bool OpenFileWithStandardProgram(string Path)
        {
            try
            {
                System.Diagnostics.Process.Start(Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region UIEventHandlers

        #region ActiveInteraction
        private void ShowUpdateRequired()
        {
            MessageBox.Show("Eine oder mehrere Speicherdateien konnten nicht gelesen werden, da sie von einer neueren Version dieses Programmes erstellt wurden. Bitte installieren Sie die neueste Version dieses Programms, um die Dateien zu öffnen", "Fehler beim Einlesen einer Filetag-Datei", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
        }

        #endregion

        #region fileNavigation

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveState();
            LastSelectedFileIndex = Files.SelectedIndex;
            //MessageBox.Show("selectedIndex Set");
            AdditionalTag.Clear();
            //MessageBox.Show(ActiveFiles.Count.ToString() + Files.SelectedIndex.ToString());
            if (ActiveFiles.Count != 0)
            {
                foreach (FileT t in ActiveFiles[Files.SelectedIndex].Tags)
                {
                    /*if(t.Type==FileT.TagType.Sonstiges)*/
                    AdditionalTag.Add(t);
                }
            }
        }

        private void Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!OpenFileWithStandardProgram(ActiveFiles[Files.SelectedIndex].CompletePath)) MessageBox.Show("The File " + ActiveFiles[Files.SelectedIndex].CompletePath + " could not be opened.");
        }

        private void Folderbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Folderbox.SelectedIndex == -1) return;
            SetCurrentFolder(Path.GetFullPath(ActiveFolders[Folderbox.SelectedIndex]));
            GetFolders();
            GetFiles();
        }

        #endregion

        #region searchbar
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<FileWithTagString> searchResults = TagSearch.Search(SearchBar.Text, FileTags);
            SearchResults.Clear();
            foreach (var x in searchResults)
            {
                SearchResults.Add(x);
            }
        }

        #endregion

        #region possibleAutoSavePoints
        private void AdditionalTags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SaveState();
        }

        private void AdditionalTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //WriteLastChange();
            //UpdateFiles();
        }

        #endregion

        #region Buttons

        private void Folder_Up_Click(object sender, RoutedEventArgs e)
        {
            SaveState();

            if (Path.GetDirectoryName(GetCurrentFolder()) != null)
            {
                SetCurrentFolder(Path.GetDirectoryName(GetCurrentFolder()));
                GetFolders();
                GetFiles();
            }
            else
            {
                ActiveFolders.Clear();
                foreach (string s in Directory.GetLogicalDrives())
                {
                    ActiveFolders.Add(s);
                }
                ActiveFiles.Clear();
            }
        }

        private void BigSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow searchWindow = new SearchWindow(DataStructureVersion);
            searchWindow.ShowDialog();
        }

        #region bottom

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AdditionalTag.Add(new FileT("NewItem", true, FileT.TagType.Sonstiges));
            SaveState();
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            AdditionalTag.RemoveAt(AdditionalTags.SelectedIndex);
            SaveState();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveState();
        }

        #endregion

        #endregion

        #endregion

    }
}