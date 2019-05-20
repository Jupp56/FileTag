using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTag
{
    class FileWithTagString
    {
        public string Name { get; set; }
        private List<FileT> Tags;
        public string TagString { get; private set; }

        public FileWithTagString(string Name, List<FileT> Tags)
        {
            this.Name = Name;
            this.Tags = Tags;
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

    }
}
