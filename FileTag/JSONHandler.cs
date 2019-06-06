using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

                if (tagDirectory is null)
                {
                    tagDirectory = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                }

                if (tagDirectory is null)
                {
                    return new List<FileWithTagString>();
                }

                //Search only for the wanted directory in the tagDirectory object, which contains all directories on this drive with tags in them

                List<string> paths = BuildAllSubdirPaths(Folder);
                paths.RemoveAt(0);
                foreach (string partPath in paths)
                {
                    tagDirectory = tagDirectory.SubDirectories.Find(x => x.Name == partPath);
                }
                if (tagDirectory is null) return new List<FileWithTagString>();
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

                if (SaveState is null) SaveState = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            }

            catch
            {
                return false;
            }

            //TODO: Proper check whether a save file or Directory even exists - This is a hack

            if (!(SaveState is null)) SaveState.AddOrReplaceDirectory(newSubDir);

            else
            {
                SaveState = new TagDirectory(new List<TagDirectory>(), new List<FileWithTagString>(), CurrentDrive);

                SaveState.AddOrReplaceDirectory(newSubDir);
            }

            try
            {
                File.WriteAllText(Path.Combine(CurrentDrive, MetaFile), JsonConvert.SerializeObject(SaveState, Formatting.Indented));

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

        public static List<string> BuildAllSubdirPaths(string path)
        {
            //Creates a complete, valid path for each or the subdirectories in the path. So when given "C:/users/exampleuser", it outputs the list {"C:/users", "C:/users/exampleuser}

            string[] subpaths = path.Split('\\');
            List<string> results = new List<string>();
            for (int i = 0; i < subpaths.Length; i++)
            {
                string partstring = "";

                for (int j = 0; j <= i; j++)
                {

                    if (j == 0)
                    {
                        partstring = subpaths[j] + "\\";
                    }
                    else if(j == 1)
                    {
                        partstring += subpaths[j];
                        
                    }
                    else
                    {
                        string[] pathstomerge = { partstring, subpaths[j] };
                        partstring = string.Join("\\", pathstomerge);
                    }
                   
                }

                results.Add(partstring);
            }

            return results;
        }
    }

    /// <summary>
    /// Class that represents a directory, with files (FileWithTagString) and subdirectories in it. New directory needed for every volume.
    /// </summary>
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

        /// <summary>
        /// Adds or replaces a directory in the TagDirectory file structure
        /// </summary>
        /// <param name="tagDirectory"></param>
        /// <returns>bool successful (only for internal recursive use)</returns>
        public bool AddOrReplaceDirectory(TagDirectory tagDirectory)
        {
            //check, if this directory is the directory to replace
            if (Name == tagDirectory.Name)
            {
                SetSubdirectories(tagDirectory.SubDirectories);
                SetFiles(tagDirectory.Files);

                return true;
            }
            //check if this directory is not a parent of the directory to replace
            else if (!tagDirectory.Name.Contains(Name))
            {
                return false;
            }
            //if this directory is a parent of the directory to replace
            else
            {
                //look if one of its subdirs contains or is the directory, use ReplaceDirectory on it
                foreach (TagDirectory subdir in SubDirectories)
                {
                    if (tagDirectory.Name.Contains(subdir.Name))
                    {
                        return subdir.AddOrReplaceDirectory(tagDirectory);
                    }
                }

                //if not, create a new subdirectory

                //build the path of the tagDirectory to insert, but without the last folder (i.e. C:/users instead of C:/users/myuser)
                List<string> subpaths = JSONHandler.BuildAllSubdirPaths(tagDirectory.Name);
                string pathwithoutlast = subpaths[subpaths.Count - 2];

                //if afforementioned subdir is the directory we want to insert, do it
                if (Name == pathwithoutlast)
                {
                    SubDirectories.Add(tagDirectory);
                    return true;
                }
                //if it is just a parent of that subdir, create a new subdir and recursively call this method on that subdir
                else
                {
                    SubDirectories.Add(new TagDirectory(null, null, pathwithoutlast));

                    SubDirectories.Find(x => x.Name == pathwithoutlast).AddOrReplaceDirectory(tagDirectory);

                    return true;
                }
            }
        }

        public void SetSubdirectories(List<TagDirectory> Subdirs)
        {
            if (Subdirs != null) SubDirectories = Subdirs;
        }

        public void SetFiles(List<FileWithTagString> Files)
        {
            if (Files != null) this.Files = Files;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            TagDirectory ItemToCompare = obj as TagDirectory;

            if (ItemToCompare == null) return false;

            bool result = false;
            try
            {
                result = ItemToCompare.Name == Name && ItemToCompare.Files == Files && ItemToCompare.SubDirectories == SubDirectories;
            }
            catch { }

            return result;
        }

        public static bool operator ==(TagDirectory left, TagDirectory right)
        {

            if (left is null || right is null) return false;

            bool result = false;
            try
            {
                result = left.Name == right.Name && left.Files == right.Files && left.SubDirectories == right.SubDirectories;
            }
            catch { }

            return result;
        }

        public static bool operator !=(TagDirectory left, TagDirectory right)
        {
            bool result = true;
            try
            {
                result = left.Name != right.Name && left.Files != right.Files && left.SubDirectories != right.SubDirectories;
            }
            catch { }

            return result;
        }
    }

    class SaveObject
    {
        public TagDirectory TagDirectory = new TagDirectory();
        public string Version;

        public SaveObject(TagDirectory tagDirectory, string version)
        {
            TagDirectory = tagDirectory;
            Version = version;
        }
    }
}