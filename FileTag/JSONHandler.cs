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
        /// <summary>
        /// Returns the filetags saved in the JSON save file for a specific directory in the file system
        /// </summary>
        /// <param name="CurrentDrive"></param>
        /// <param name="MetaFile"></param>
        /// <param name="Folder"></param>
        /// <returns>List of FileWithTagString</returns>
        public static List<FileWithTagString> ReadJSONInfoFromDirectory(string CurrentDrive, string MetaFile, string Folder, int dataStructureVersion)
        {
            TagDirectory tagDirectory = new TagDirectory();

            try
            {
                //tries to read from the standard location (root directory of volume)
                tagDirectory = ReadJSONInfo(Path.Combine(CurrentDrive, MetaFile), dataStructureVersion);

                if (tagDirectory is null)
                {
                    //tries to read from MyDocuments if root dir is inaccessible
                    tagDirectory = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MetaFile), dataStructureVersion);
                }

                if (tagDirectory is null)
                {
                    //if still no file found, an empty set is returned
                    return new List<FileWithTagString>();
                }

                //Filter everything but the wanted directory out of the tagDirectory object, which up until now contains all directories on this drive with tags in them

                List<string> paths = BuildAllSubdirPaths(Folder);
                paths.RemoveAt(0);
                foreach (string partPath in paths)
                {
                    tagDirectory = tagDirectory.SubDirectories.Find(x => x.Name == partPath);
                }
                //if file existed but nothing was found (i.e. empty)
                if (tagDirectory is null) return new List<FileWithTagString>();
            }
            catch
            {
                return new List<FileWithTagString>();
            }

            return tagDirectory.Files;
        }

        /// <summary>
        /// Reads the JSON save file from a given path
        /// </summary>
        /// <param name="PathToJSON"></param>
        /// <returns>Returns TagDirectory with all FileWithTagString on the drive</returns>
        public static TagDirectory ReadJSONInfo(string PathToJSON, int version)
        {
            SaveObject saveObject = null;

            try
            {
                if (File.Exists(PathToJSON))
                {
                    saveObject = JsonConvert.DeserializeObject<SaveObject>(File.ReadAllText(PathToJSON));
                }
            }
            catch
            {

            }

            if (saveObject?.Version < version)
            {
                //currently of course not working, as it will most probably crash when casted to the SaveObject type when JSON-deserializing
                VersionUpdate(saveObject);

            }
            else if (saveObject?.Version > version)
            {
                throw new DataStructureVersionToHighException("Version of savefile newer than program version. Update required");
            }

            return saveObject?.TagDirectory;
        }

        /// <summary>
        /// Updates the database to a new data structure, if changes occured to the data structure
        /// </summary>
        /// <param name="saveObject"></param>
        private static void VersionUpdate(SaveObject saveObject)
        {
            return;
        }

        /// <summary>
        /// Writes the FilesWithTags into the JSON save file
        /// </summary>
        /// <param name="currentDrive"></param>
        /// <param name="metaFile"></param>
        /// <param name="directoryToUpdate"></param>
        /// <param name="filesWithTags"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool WriteJSONInfo(string currentDrive, string metaFile, string directoryToUpdate, List<FileWithTagString> filesWithTags, int version)
        {

            TagDirectory SaveState = new TagDirectory();

            TagDirectory newSubDir = new TagDirectory(null, filesWithTags, directoryToUpdate);

            try
            {
                SaveState = ReadJSONInfo(Path.Combine(currentDrive, metaFile), version);

                if (SaveState is null) SaveState = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), version);
            }

            catch
            {
                return false;
            }

            if (!(SaveState is null)) SaveState.AddOrReplaceDirectory(newSubDir);

            else
            {
                SaveState = new TagDirectory(new List<TagDirectory>(), new List<FileWithTagString>(), currentDrive);

                SaveState.AddOrReplaceDirectory(newSubDir);
            }

            SaveObject saveObject = new SaveObject(SaveState, version);

            try
            {

                File.WriteAllText(Path.Combine(currentDrive, metaFile), JsonConvert.SerializeObject(saveObject, Formatting.Indented));

                return true;
            }
            catch
            {
                try
                {
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), metaFile), JsonConvert.SerializeObject(saveObject, Formatting.Indented));

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates a complete, valid path for each or the subdirectories in the path. So when given "C:/users/exampleuser", it outputs the list {"C:/users", "C:/users/exampleuser}
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> BuildAllSubdirPaths(string path)
        {

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
                    else if (j == 1)
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
        [JsonProperty("SD")]
        public List<TagDirectory> SubDirectories { get; private set; } = new List<TagDirectory>();

        [JsonProperty("F")]
        public List<FileWithTagString> Files { get; private set; } = new List<FileWithTagString>();

        [JsonProperty("N")]
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
                //SetSubdirectories(tagDirectory.SubDirectories); Muss man ja nicht setzen, da immer nur ein Dir auf einer Ebene gleichzeitig gesetzt wird.
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
                    //create the next missing subdir (one subdir deeper in file structure)
                    string newSubdirName = "";
                    for (int i = 0; i<subpaths.Count; i++)
                    {
                        if (subpaths[i] == Name) newSubdirName = subpaths[i + 1];
                    }

                    SubDirectories.Add(new TagDirectory(null, null, newSubdirName));

                    SubDirectories.Find(x => x.Name == newSubdirName).AddOrReplaceDirectory(tagDirectory);

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
        public int Version;

        public SaveObject(TagDirectory tagDirectory, int version)
        {
            TagDirectory = tagDirectory;
            Version = version;
        }
    }

    public class DataStructureVersionToHighException : Exception
    {
        public DataStructureVersionToHighException() { }
        public DataStructureVersionToHighException(string message) : base(message) { }
        public DataStructureVersionToHighException(string message, Exception inner) : base(message, inner) { }
        protected DataStructureVersionToHighException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}