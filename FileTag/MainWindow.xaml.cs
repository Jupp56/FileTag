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

namespace FileTag
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string CurrentFolder;
        string MetaFile = ".FileTag";
        private ObservableCollection<FSItem> ActiveFolders = new ObservableCollection<FSItem>();
        private ObservableCollection<FileT> AdditionalTag = new ObservableCollection<FileT>();
        private List<FileWithTags> FileTags = new List<FileWithTags>();

        public MainWindow()
        {
            //#REMOVE
            CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            GetFolders();
            InitializeComponent();
            
            Folders.ItemsSource = ActiveFolders;
            AdditionalTags.ItemsSource = AdditionalTag;
            //WriteJSONInfo();


        }

        private void GetFolders()
        {
            ActiveFolders.Clear();
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
                ActiveFolders.Add(new FSItem(f.Name, f.Length, f.LastWriteTime, LookupTags(f.Name)));
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
            FileTags = JsonConvert.DeserializeObject<List<FileWithTags>>(File.ReadAllText(System.IO.Path.Combine(CurrentFolder, MetaFile)));
        }

        private void WriteJSONInfo()
        {
            File.WriteAllText(System.IO.Path.Combine(CurrentFolder, MetaFile), JsonConvert.SerializeObject(FileTags));
            MessageBox.Show("Written to: " + System.IO.Path.Combine(CurrentFolder, MetaFile));
        }

        private void Folders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdditionalTag.Clear();
            //MessageBox.Show(Folders.SelectedItem.ToString());
            foreach(FileT t in ActiveFolders[Folders.SelectedIndex].Tags)
            {
                /*if(t.Type==FileT.TagType.Sonstiges)*/ AdditionalTag.Add(t);

            }
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
