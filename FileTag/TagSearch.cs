using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTag
{
    class TagSearch
    {
        /// <summary>
        /// For search window search
        /// </summary>
        /// <param name="searchObjects"></param>
        /// <param name="filesWithTags"></param>
        /// <returns></returns>
        public static List<FileWithTagString> Search(List<SearchObject> searchObjects, List<FileWithTagString> filesWithTags)
        {
            List<FileWithTagString> searchResults = new List<FileWithTagString>();

            foreach (SearchObject searchObject in searchObjects)
            {
                if (searchObject.TagName == string.Empty) continue;
                List<FileWithTagString> searchResult = Search(searchObject.TagName, filesWithTags, false);

                switch (searchObject.Junction)
                {
                    case (SearchJunction.und):
                        searchResults = searchResults.Intersect(searchResult).ToList();
                        break;
                    case (SearchJunction.oder):
                        searchResults = searchResults.Union(searchResult).ToList();
                        break;
                    case (SearchJunction.nicht):
                        searchResults = searchResults.Union(new List<FileWithTagString>(filesWithTags).Where(x => !searchResult.Contains(x))).ToList();
                        break;
                    case (SearchJunction.undNicht):
                        searchResults.RemoveAll(x => searchResult.Contains(x));
                        break;
                }
            }
            //searchResults = RemoveDoubles(searchResults);
            return searchResults;
        }
        /// <summary>
        /// For quicksearchbar and as helping struct for searchwindow search
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="filesWithTags"></param>
        /// <param name="allowMultipleTagsWithWhiteSpaces"></param>
        /// <returns></returns>
        public static List<FileWithTagString> Search(string searchString, List<FileWithTagString> filesWithTags, bool allowMultipleTagsWithWhiteSpaces)
        {
            string[] emptyset = { string.Empty };
            string[] tagstosearch;

            if (allowMultipleTagsWithWhiteSpaces)
            {
                
                tagstosearch = searchString.Split(' ').Except(emptyset).ToArray();
            }
            else
            {
                tagstosearch = new string[] { searchString };
            }
            return Search(tagstosearch, filesWithTags);
        }

        /// <summary>s
        /// Actual class-internal search method
        /// </summary>
        /// <param name="tagstosearch"></param>
        /// <param name="filesWithTags"></param>
        /// <returns></returns>
        private static List<FileWithTagString> Search(string[] tagstosearch, List<FileWithTagString> filesWithTags)
        {
            List<FileWithTagString> results = new List<FileWithTagString>();

            foreach (string s in tagstosearch)
            {
                if (s != string.Empty)
                {
                    foreach (FileWithTagString fwt in filesWithTags.FindAll(x => x.Tags.Exists(y => y.Value.Contains(s)) || x.Name.Contains(s)))
                    {
                        results.Add(new FileWithTagString(fwt.Name, fwt.Tags));
                    }
                }
            }
            results = RemoveDoubles(results);
            return results;

        }

        private static List<FileWithTagString> RemoveDoubles(List<FileWithTagString> results)
        {
            return results.Distinct().ToList();
        }

        public static List<FileWithTagString> GetSystemWideTags(int dataStructureVersion)
        {
            List<FileWithTagString> results = new List<FileWithTagString>();
            foreach (string s in Environment.GetLogicalDrives())
            {
                try
                {
                    TagDirectory tagDir = JSONHandler.ReadJSONInfo(Path.Combine(s, MainWindow.MetaFile), dataStructureVersion);
                    if(tagDir is null) tagDir = JSONHandler.ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MainWindow.MetaFile), dataStructureVersion);
                    results = ExtractAllTagsFromTagDir(tagDir);
                }
                catch { }
            }
            return results;
        }

        private static List<FileWithTagString> ExtractAllTagsFromTagDir(TagDirectory tagDir)
        {
            List<FileWithTagString> results = new List<FileWithTagString>();
            foreach (FileWithTagString f in tagDir.Files)
            {
                results.Add(f);
            }

            foreach (TagDirectory subDir in tagDir.SubDirectories)
            {
                results.AddRange(ExtractAllTagsFromTagDir(subDir));
            }

            return results;
        }

        public enum SearchJunction
        {
            oder,
            und,
            nicht,
            undNicht
        }
    }

    class SearchObject
    {
        public TagSearch.SearchJunction Junction;
        public FileT.TagType TagType;
        public string TagName;

        public SearchObject(TagSearch.SearchJunction junction, FileT.TagType tagType, string tagName)
        {
            Junction = junction;
            TagType = tagType;
            TagName = tagName;
        }
    }
}
