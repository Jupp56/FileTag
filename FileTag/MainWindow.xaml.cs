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

            //WriteJSONInfo();

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

            FileTags = JSONHandler.ReadJSONInfoFromDirectory(CurrentDrive, MetaFile, GetCurrentFolder());

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

        private List<FileT> LookupTags(string filename)
        {
            try
            {
                return FileTags.Find(x => x.FullName == filename).Tags;
            }
            catch { return null; }
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

        private void SaveState()
        {
            if (ActiveFiles.Count > 0)
            {
                UpdateFiles();
                UpdateFileTags();
                JSONHandler.WriteJSONInfo(CurrentDrive, MetaFile, CurrentFolder, FileTags);
            }
        }

        private void Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!OpenFileWithStandardProgram(ActiveFiles[Files.SelectedIndex].CompletePath)) MessageBox.Show("The File " + ActiveFiles[Files.SelectedIndex].CompletePath + " could not be opened.");
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

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AdditionalTag.Add(new FileT("NewItem", true, FileT.TagType.Sonstiges));
            SaveState();
        }

        private void AdditionalTags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SaveState();
        }

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

        private void Folderbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetCurrentFolder(Path.GetFullPath(ActiveFolders[Folderbox.SelectedIndex]));
            GetFolders();
            GetFiles();
        }

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

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<FileWithTagString> searchResults = TagSearch.Search(SearchBar.Text, FileTags);
            SearchResults.Clear();
            foreach (var x in searchResults)
            {
                SearchResults.Add(x);
            }
        }

        private void AdditionalTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //WriteLastChange();
            //UpdateFiles();
        }

        private void BigSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow searchWindow = new SearchWindow();
            searchWindow.ShowDialog();
        }
    }
}