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
        string CurrentFolder;
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
        string MetaFile = ".FileTag";

        private ObservableCollection<string> ActiveFolders = new ObservableCollection<string>();
        private ObservableCollection<FSItem> ActiveFiles = new ObservableCollection<FSItem>();

        private ObservableCollection<FileT> AdditionalTag = new ObservableCollection<FileT>();
        private List<FileWithTags> FileTags = new List<FileWithTags>();
        private ObservableCollection<FileWithTagString> SearchResults = new ObservableCollection<FileWithTagString>();
        private int LastSelectedFolderIndex = 0;

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
            ReadJSONInfo();
            string[] Files = Directory.GetFiles(GetCurrentFolder());
            List<FileInfo> FileInfos = new List<FileInfo>();
            foreach (string f in Files)
            {
                FileInfos.Add(new FileInfo(Path.Combine(GetCurrentFolder(), f)));
            }
            foreach (FileInfo f in FileInfos)
            {
                ActiveFiles.Add(new FSItem(f.FullName, f.Length, f.LastWriteTime, LookupTags(f.FullName)));
            }
        }

        private List<FileT> LookupTags(string filename)
        {
            try
            {
                return FileTags.Find(x => x.Name == filename).Tags;
            }
            catch { return null; }
        }

        private void ReadJSONInfo()
        {
            FileTags.Clear();
            try
            {

                if (File.Exists(Path.Combine(CurrentFolder, MetaFile)))
                {
                    FileTags = JsonConvert.DeserializeObject<List<FileWithTags>>(File.ReadAllText(Path.Combine(CurrentFolder, MetaFile)));
                }
                else
                {
                    FileTags = JsonConvert.DeserializeObject<List<FileWithTags>>(File.ReadAllText(Path.Combine(CurrentDrive, MetaFile)));
                }
            }
            catch
            {

            }
        }

        private void WriteJSONInfo()
        {
            try
            {
                File.WriteAllText(Path.Combine(CurrentDrive, MetaFile), JsonConvert.SerializeObject(FileTags));
            }
            catch
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MetaFile), JsonConvert.SerializeObject(FileTags));
            }
            //MessageBox.Show("Written to: " + System.IO.Path.Combine(CurrentFolder, MetaFile));
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AdditionalTag.Add(new FileT("NewItem", true, FileT.TagType.Sonstiges));
        }

        private void AdditionalTags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            WriteLastChange();
        }

        private void WriteLastChange()
        {
            try
            {
                foreach (FileT tag in AdditionalTag)
                {
                    ActiveFiles[LastSelectedFolderIndex].AddTag(tag);
                }
                FileTags.Clear();

                foreach (FSItem fsItem in ActiveFiles)
                {
                    if (fsItem.Tags.Count > 0) FileTags.Add(new FileWithTags(fsItem.CompletePath, fsItem.Tags));
                }

            }
            catch { }
        }

        private void Search(string searchString)
        {
            string[] tagstosearch = searchString.Split(' ');
            SearchResults.Clear();
            //MessageBox.Show("Searching: " + searchString);
            foreach (string s in tagstosearch)
            {
                if (s != string.Empty)
                {
                    foreach (FileWithTags fwt in FileTags.FindAll(x => x.Tags.Exists(y => y.Value.Contains(s))))
                    {
                        SearchResults.Add(new FileWithTagString(Path.GetFileName(fwt.Name), fwt.Tags));
                    }
                }
            }
           
        }
        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WriteLastChange();
            WriteJSONInfo();
            LastSelectedFolderIndex = Files.SelectedIndex;
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

        private void SearchBar_TextInput(object sender, TextCompositionEventArgs e)
        {
            Search(SearchBar.Text);
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Search(SearchBar.Text);
        }
    }
}