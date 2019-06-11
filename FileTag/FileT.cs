using System.Windows;
using Newtonsoft.Json;

namespace FileTag
{
    internal class FileT
    {
        [JsonProperty("V")]
        public string Value { get; set; }
        [JsonProperty("SV")]
        public bool StdVisible { get; set; }
        [JsonProperty("TT")]
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