using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace FileTag
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private ObservableCollection<FileWithTagString> searchResults = new ObservableCollection<FileWithTagString>();
        public SearchWindow()
        {
            InitializeComponent();
            Title = "Erweiterte Suche";

            Type1.ItemsSource = Enum.GetValues(typeof(FileT.TagType));
            Type2.ItemsSource = Enum.GetValues(typeof(FileT.TagType));
            Type3.ItemsSource = Enum.GetValues(typeof(FileT.TagType));
            Type4.ItemsSource = Enum.GetValues(typeof(FileT.TagType));
            And1.ItemsSource = Enum.GetValues(typeof(TagSearch.SearchJunction));
            And2.ItemsSource = Enum.GetValues(typeof(TagSearch.SearchJunction));
            And3.ItemsSource = Enum.GetValues(typeof(TagSearch.SearchJunction));
            And4.ItemsSource = Enum.GetValues(typeof(TagSearch.SearchJunction));

            Type1.SelectedIndex = 0;
            Type2.SelectedIndex = 0;
            Type3.SelectedIndex = 0;
            Type4.SelectedIndex = 0;
            And1.SelectedIndex = 0;
            And2.SelectedIndex = 0;
            And3.SelectedIndex = 0;
            And4.SelectedIndex = 0;


            SearchResults.ItemsSource = searchResults;
            //searchResults.Add(new FileWithTagString("blah", new List<FileT>() { new FileT("blub", true, FileT.TagType.Anlass) }));
        }

        private void Searchterm1_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchResults.Clear();

            List<SearchObject> searchObjects = new List<SearchObject>
            {
                new SearchObject((TagSearch.SearchJunction)And1.SelectedIndex, (FileT.TagType)Type1.SelectedIndex, Searchterm1.Text),
                new SearchObject((TagSearch.SearchJunction)And2.SelectedIndex, (FileT.TagType)Type2.SelectedIndex, Searchterm2.Text),
                new SearchObject((TagSearch.SearchJunction)And3.SelectedIndex, (FileT.TagType)Type3.SelectedIndex, Searchterm3.Text),
                new SearchObject((TagSearch.SearchJunction)And4.SelectedIndex, (FileT.TagType)Type4.SelectedIndex, Searchterm4.Text)
            };

            List<FileWithTagString> Results = TagSearch.Search(searchObjects, TagSearch.GetSystemWideTags());

            foreach (FileWithTagString fileWithTagString in Results)
            {
                searchResults.Add(fileWithTagString);
            }
        }


    }
}
