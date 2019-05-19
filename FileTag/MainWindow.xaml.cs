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
        string MetaFile = ".FileTag";

        private ObservableCollection<string> ActiveFolders = new ObservableCollection<string>();
        private ObservableCollection<FSItem> ActiveFiles = new ObservableCollection<FSItem>();

        private ObservableCollection<FileT> AdditionalTag = new ObservableCollection<FileT>();
        private List<FileWithTags> FileTags = new List<FileWithTags>();
        private int LastSelectedFolderIndex = 0;

        public MainWindow()
        {
            //#REMOVE
            CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
           
            GetFolders();
            GetFiles();
            InitializeComponent();
            Folderbox.ItemsSource = ActiveFolders;
            Files.ItemsSource = ActiveFiles;
            AdditionalTags.ItemsSource = AdditionalTag;

            //WriteJSONInfo();

        }

        private void GetFolders()
        {
            ActiveFolders.Clear();
            foreach (string s in Directory.GetDirectories(CurrentFolder))
            {
                ActiveFolders.Add(s);
            }

        }
        private void GetFiles()
        {
            ActiveFiles.Clear();
            ReadJSONInfo();
            string[] Files = Directory.GetFiles(CurrentFolder);
            List<FileInfo> FileInfos = new List<FileInfo>();
            foreach (string f in Files)
            {
                FileInfos.Add(new FileInfo(System.IO.Path.Combine(CurrentFolder, f)));
            }
            foreach (FileInfo f in FileInfos)
            {
                //Clash zwischen Name und lookup
                ActiveFiles.Add(new FSItem(f.Name, f.Length, f.LastWriteTime, LookupTags(f.Name)));
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
                FileTags = JsonConvert.DeserializeObject<List<FileWithTags>>(File.ReadAllText(Path.Combine(CurrentFolder, MetaFile)));
            }
            catch { }
        }

        private void WriteJSONInfo()
        {
            File.WriteAllText(System.IO.Path.Combine(CurrentFolder, MetaFile), JsonConvert.SerializeObject(FileTags));
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
                foreach (FileT t in AdditionalTag)
                {
                    ActiveFiles[LastSelectedFolderIndex].AddTag(t);
                }

            }
            catch { }
        }

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WriteLastChange();
            WriteJSONInfo();
            LastSelectedFolderIndex = Files.SelectedIndex;
            AdditionalTag.Clear();

            foreach (FileT t in ActiveFiles[Files.SelectedIndex].Tags)
            {
                /*if(t.Type==FileT.TagType.Sonstiges)*/
                AdditionalTag.Add(t);
            }
        }

        private void Folderbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CurrentFolder = Path.GetFullPath(ActiveFolders[Folderbox.SelectedIndex]);
            GetFolders();
            GetFiles();
        }

        private void Folder_Up_Click(object sender, RoutedEventArgs e)
        {
            if (Path.GetDirectoryName(CurrentFolder) != null)
            {
                CurrentFolder = Path.GetDirectoryName(CurrentFolder);
                GetFolders();
                GetFiles();
            }
            else
            {
                ActiveFolders.Clear();
                foreach(string s in Directory.GetLogicalDrives())
                {
                    ActiveFolders.Add(s);
                }
                ActiveFiles.Clear();
            }
           
        }
    }
}