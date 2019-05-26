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
        public static List<FileWithTagString> ReadJSONInfoFromDirectory(string CurrentDrive, string MetaFile, string Folder)
        {
            TagDirectory tagDirectory = new TagDirectory();

            

            try
            {

                tagDirectory = ReadJSONInfo(Path.Combine(CurrentDrive, MetaFile));

                if (tagDirectory == null)
                {
                    tagDirectory = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                }

                if (tagDirectory == null)
                {
                    return new List<FileWithTagString>();
                }

                //Search only for the wanted directory in the tagDirectory object, which contains all directories on this drive with tags in them
                string[] subdirs = Folder.Split('\\');

                //Builds back a complete, valid path for each the subdirectories generated before. So it i.e. searches for "C:/users/exampleuser" instead of "exampleuser"
                for (int i = 0; i < subdirs.Length; i++)
                {
                    string partOfPath = "";

                    for (int j = 0; j <= i; j++)
                    {
                        partOfPath = Path.Combine(partOfPath, subdirs[j]);
                    }

                    tagDirectory = tagDirectory.SubDirectories.Find(x => x.Name == partOfPath);
                }
            }
            catch
            {
                return new List<FileWithTagString>();
            }

            return tagDirectory.Files;
        }

        public static TagDirectory ReadJSONInfo(string PathToJSON)
        {
            TagDirectory allFiles = null;

            try
            {
                if (File.Exists(PathToJSON))
                {
                    allFiles = JsonConvert.DeserializeObject<TagDirectory>(File.ReadAllText(PathToJSON));
                }
                else
                {
                    allFiles = JsonConvert.DeserializeObject<TagDirectory>(File.ReadAllText(PathToJSON));
                }
            }
            catch
            {

            }

            return allFiles;
        }

        public static bool WriteJSONInfo(string CurrentDrive, string MetaFile, string DirectoryToUpdate, List<FileWithTagString> FilesWithTags)
        {
           
            TagDirectory SaveState = new TagDirectory();

            TagDirectory newSubDir = new TagDirectory(null, FilesWithTags, DirectoryToUpdate);

            try
            {
                SaveState = ReadJSONInfo(Path.Combine(CurrentDrive, MetaFile));

                if (SaveState == null) SaveState = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            }

            catch
            {
                return false;
            }

            SaveState.ReplaceDirectory(newSubDir);

            try
            {
                File.WriteAllText(Path.Combine(CurrentDrive, MetaFile), JsonConvert.SerializeObject(SaveState));

                return true;
            }
            catch
            {
                try
                {
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MetaFile), JsonConvert.SerializeObject(SaveState));

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    class TagDirectory
    {
        public List<TagDirectory> SubDirectories { get; private set; } = new List<TagDirectory>();

        public List<FileWithTagString> Files { get; private set; } = new List<FileWithTagString>();

        ///complete path of that directory. More space-consuming but easier and faster to process.
        public string Name;

        public TagDirectory()
        {

        }

        public TagDirectory(List<TagDirectory> SubDirectories, List<FileWithTagString> Files, string Name)
        {
            if (SubDirectories != null) this.SubDirectories = SubDirectories;
            if (Files != null) this.Files = Files;
            this.Name = Name;
        }

        public bool ReplaceDirectory(TagDirectory tagDirectory)
        {
            if (Name == tagDirectory.Name)
            {
                SetSubdirectories(tagDirectory.SubDirectories);
                SetFiles(tagDirectory.Files);
                return true;
            }
            else
            {
                foreach(TagDirectory subdir in SubDirectories)
                {
                    try
                    {
                        if (subdir.ReplaceDirectory(tagDirectory)) return true;                 
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        public void SetSubdirectories(List<TagDirectory> Subdirs)
        {
            if (Subdirs != null) this.SubDirectories = Subdirs;
        }

        public void SetFiles(List<FileWithTagString> Files)
        {
            if (Files != null) this.Files = Files;
        }
    }
}
