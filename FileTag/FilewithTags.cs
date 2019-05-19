using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTag
{
    class FileWithTags
    {
        public string Name { get; set; }
        public List<FileT> Tags { get; set; }

        public FileWithTags(string Name, List<FileT> Tags)
        {
            this.Name = Name;
            this.Tags = Tags;
        }
    }
}