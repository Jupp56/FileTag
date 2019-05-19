using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileTag
{
    class FSItem
    {
        public string FileName { get; set; }
        public string FileSize { get; private set; }
        public string FileType { get; private set; }
        public DateTime LastChanged { get; set; }
        public List<FileT> Tags { get; private set; } = new List<FileT>();
        public string TagString { get; private set; }

        public FSItem(string FileName, Int64 FileSizeInBytes, DateTime LastChanged, List<FileT> tags)
        {
            this.FileName = FileName;
            SetFileSize(FileSizeInBytes);
            this.LastChanged = LastChanged;
            if(tags!=null) this.Tags = tags;
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

        private void SearchForTagDuplicates()
        {

            List<FileT> oldTags = new List<FileT>(Tags);
            Tags.Clear();
            foreach(FileT t in oldTags)
            {
                if(!Tags.Contains(t)) Tags.Add(t);
            }
            
        }
        private void SetFileSize(Int64 FileSizeInBytes)
        {
            string FileEnding = "B";
            Int64 FileSizeChopped = FileSizeInBytes;


            if (FileSizeInBytes > 1000000000000000)
            {
                FileSizeInBytes = FileSizeInBytes / 1000000000000000;
                FileEnding = "PB";
            }
            if (FileSizeInBytes > 1000000000)
            {
                FileSizeInBytes = FileSizeInBytes / 1000000000;
                FileEnding = "GB";
            }
            if (FileSizeInBytes > 1000000)
            {
                FileSizeInBytes = FileSizeInBytes / 1000000;
                FileEnding = "MB";
            }
            if (FileSizeInBytes > 1000)
            {
                FileSizeInBytes = FileSizeInBytes / 1000;
                FileEnding = "KB";
            }

            this.FileSize = FileSizeChopped.ToString() + " " + FileEnding;
        }
        private void SetFileType()
        {
            try
            {
                FileType = Tags.Find(x => x.Type == FileT.TagType.Filetype).Value;
            }
            catch (Exception ex) {  }
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