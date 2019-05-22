using System;
using System.Collections.Generic;
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
        public List<FileT> Tags { get; private set; } = new List<FileT>();
        public string TagString { get; private set; }

        public FSItem(string CompletePath, Int64 FileSizeInBytes, DateTime LastChanged, List<FileT> Tags)
        {
            this.FileName = Path.GetFileName(CompletePath);
            this.CompletePath = CompletePath;
            SetFileSize(FileSizeInBytes);
            this.LastChanged = LastChanged;
            if (Tags != null) this.Tags = Tags;
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
            this.Tags = tags;
            SearchForTagDuplicates();
            RebuildTagString();
        }

        public void SetTags(List<FileT> tags)
        {
            if (tags != null)
            {
                Tags = tags;
                SearchForTagDuplicates();
                RebuildTagString();
            }
            else
            {
                Tags.Clear();
            }
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
                FileType = Tags.Find(x => x.Type == FileT.TagType.Filetype).Value;
            }
            catch (Exception ex) { }
        }
        private void RebuildTagString()
        {
            SearchForTagDuplicates();
            TagString = "";
            foreach (FileT t in Tags)
            {
                if (t.Type != FileT.TagType.Filetype)
                    TagString += t.Value + "; ";
            }
        }
    }
}