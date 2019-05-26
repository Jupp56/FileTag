using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTag
{
    class FileWithTagString
    {
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public List<FileT> Tags { get; private set; }
        public string TagString { get; private set; }

        public FileWithTagString(string FullName, List<FileT> Tags)
        {
            this.FullName = FullName;
            this.Name = Path.GetFileName(FullName);
            this.Tags = new List<FileT>(Tags);
            BuildTagString();
        }

        public void SetFullName(string FullName)
        {
            this.FullName = FullName;
            Name = Path.GetFileName(FullName);
        }

        public void SetTags(List<FileT> tags)
        {
            if (tags != null)
            {
                Tags.Clear();
                Tags = tags;
            }
            BuildTagString();
        }

        public void AddTag(FileT fileT)
        {
            Tags.Add(fileT);
            BuildTagString();
        }

        private void BuildTagString()
        {
            TagString = "";
            foreach (FileT t in Tags)
            {
                if (t.Type != FileT.TagType.Filetype)
                    TagString += t.Value + "; ";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null||!(obj is FileWithTagString)) return false;
            
            FileWithTagString param = obj as FileWithTagString;

            return (param.Name == this.Name && param.TagString == this.TagString);

        }

        public override int GetHashCode()
        {
            int hashFirstName = Name == null ? 0 : Name.GetHashCode();
            int hashLastName = TagString == null ? 0 : TagString.GetHashCode();

            return hashFirstName ^ hashLastName;
        }

        public static bool operator ==(FileWithTagString left, FileWithTagString right)
        {

            return (left.Name == right.Name && left.TagString == right.TagString);
        }
        public static bool operator !=(FileWithTagString left, FileWithTagString right)
        {
            return (left.Name != right.Name || left.TagString == right.TagString);
        }


    }
}
