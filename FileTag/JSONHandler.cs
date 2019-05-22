using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTag
{
    class JSONHandler
    {
        public static List<FileWithTagString> ReadJSONInfo(string CurrentDrive, string MetaFile)
        {
            List<FileWithTagString> FileTags = new List<FileWithTagString>();

            try
            {

                if (File.Exists(Path.Combine(CurrentDrive, MetaFile)))
                {
                    FileTags = JsonConvert.DeserializeObject<List<FileWithTagString>>(File.ReadAllText(Path.Combine(CurrentDrive, MetaFile)));
                }
                else
                {
                    FileTags = JsonConvert.DeserializeObject<List<FileWithTagString>>(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MetaFile)));
                }
            }
            catch
            {

            }

            return FileTags;
        }

        public static void WriteJSONInfo(string CurrentDrive, string MetaFile, List<FileWithTagString> FilesWithTags)
        {
            try
            {
                File.WriteAllText(Path.Combine(CurrentDrive, MetaFile), JsonConvert.SerializeObject(FilesWithTags));
            }
            catch
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MetaFile), JsonConvert.SerializeObject(FilesWithTags));
            }
        }
    }
}
