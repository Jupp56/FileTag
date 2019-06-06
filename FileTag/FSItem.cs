using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileTag
{
    class FSItem
    {
        public string FileName { get; set; }
        public string CompletePath { get; set; }
        public string FileSize { get; private set; }
        public string FileType { get; private set; }
        public DateTime LastChanged { get; set; }
        public ObservableCollection<FileT> Tags { get; private set; } = new ObservableCollection<FileT>();
        public string TagString { get; private set; }

        public FSItem(string CompletePath, long FileSizeInBytes, DateTime LastChanged, List<FileT> Tags)
        {
            this.FileName = Path.GetFileName(CompletePath);
            this.CompletePath = CompletePath;
            SetFileSize(FileSizeInBytes);
            this.LastChanged = LastChanged;
            if (Tags != null) SetTags(Tags);
            SearchForTagDuplicates();
            RebuildTagString();
            SetFileType();
        }

        public void AddTag(FileT t)
        {
            Tags.Add(t);
            RebuildTagString();
        }

        public void AddTags(List<FileT> tags)
        {
            if (tags != null)
            {
                foreach (FileT tag in tags)
                {
                    Tags.Add(tag);
                }

                SearchForTagDuplicates();
                RebuildTagString();
            }
        }

        public void SetTags(List<FileT> tags)
        {
            Tags.Clear();
            AddTags(tags);
            RebuildTagString();
        }

        private void SearchForTagDuplicates()
        {
            if (Tags != null)
            {
                List<FileT> oldTags = new List<FileT>(Tags);
                Tags.Clear();
                foreach (FileT t in oldTags)
                {
                    if (!Tags.Contains(t)) Tags.Add(t);
                }
            }
        }

        private void SetFileSize(Int64 FileSizeInBytes)
        {
            string FileEnding = "B";
            Int64 FileSizeChopped = FileSizeInBytes;

            if (FileSizeInBytes > 1000)
            {
                FileSizeChopped = FileSizeInBytes / 1000;
                FileEnding = "KB";
            }
            if (FileSizeInBytes > 1000000)
            {
                FileSizeChopped = FileSizeInBytes / 1000000;
                FileEnding = "MB";
            }
            if (FileSizeInBytes > 1000000000)
            {
                FileSizeChopped = FileSizeInBytes / 1000000000;
                FileEnding = "GB";
            }
            if (FileSizeInBytes > 1000000000000000)
            {
                FileSizeChopped = FileSizeInBytes / 1000000000000000;
                FileEnding = "PB";
            }



            FileSize = FileSizeChopped.ToString() + " " + FileEnding;
        }

        private void SetFileType()
        {
            try
            {
                FileType = Tags.First(x => x.Type == FileT.TagType.Filetype).Value;
            }
            catch  { }
        }

        /// <summary>
        /// Deletes and newly builds the tag string (use when tag list is updated)
        /// </summary>
        private void RebuildTagString()
        {
            TagString = "";
            foreach (FileT t in Tags)
            {
                if (t.Type != FileT.TagType.Filetype)
                    TagString += t.Value + "; ";
            }
        }
    }
}