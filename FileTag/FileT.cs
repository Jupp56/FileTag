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
            Filetype,
            Komponist,
            Bearbeiter,
            Genre,
            Anlass,
            Saison,
            Tonart,
            Stimmung,
            Sonstiges
        }
    }


}