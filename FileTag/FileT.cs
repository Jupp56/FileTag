using System.Windows;

namespace FileTag
{
    internal class FileT
    {
        public string Value { get; set; }
        public bool StdVisible { get; set; }
        public TagType Type { get; set; }

        public FileT(string Name, bool StdVisible, TagType Type)
        {
            this.Value = Name;
            this.StdVisible = StdVisible;
            this.Type = Type;
        }

        public enum TagType
        {
            Anlass,
            Bearbeiter,
            Besetzung,
            Filetype,
            Genre,
            Komponist,
            Saison,         
            Stimmung,
            Sonstiges,
            Tonart,
        }
    }


}