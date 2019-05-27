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

                if (tagDirectory == null)
                {
                    tagDirectory = ReadJSONInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                }

                if (tagDirectory == null)
                {
                    return new List<FileWithTagString>();
                }

                //Search only for the wanted directory in the tagDirectory object, which contains all directories on this drive with tags in them


                foreach (string partPath in BuildAllSubdirPathsWithoutFirst(Folder))
                {

                    tagDirectory = tagDirectory.SubDirectories.Find(x => x.Name == partPath);
                }
            }
            catch
            {
                return new List<FileWithTagString>();
            }

            return tagDirectory.Files;
        }

        //Builds back a complete, valid path for each the subdirectories generated before. So it i.e. searches for "C:/users/exampleuser" instead of "exampleuser"
        public static List<string> BuildAllSubdirPathsWithoutFirst(string path)
        {
            char[] patharr = path.ToArray();
            List<int> positions = new List<int>();
            for (int i = 0; i < patharr.Length; i++)
            {
                if (patharr[i] == '\\') positions.Add(i);
            }

            List<string> results = new List<string>();

            foreach(int position in positions)
            {
                results.Add(path.Substring(0, position+1));
                MessageBox.Show(path.Substring(0, position+1));
            }

            results.Add(path);
            results.RemoveAt(0);
            return results;
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

            //TODO: Proper check whether a save file or Directory even exists - This is a hack

            if (!(SaveState is null)) SaveState.ReplaceDirectory(newSubDir);

            else
            {
                SaveState = new TagDirectory(new List<TagDirectory>(), new List<FileWithTagString>(), CurrentDrive);



                // #Left here Hier stattdessen die noch zu schreibende addsubdirmethode aufrufen. Vielleicht auch besser nicht im foreach sondern intern lösen lassen.

                SaveState.AddDirectory(newSubDir);
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
                foreach (TagDirectory subdir in SubDirectories)
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

        public void AddDirectory(TagDirectory tagDirectory)
        {
            List<string> subdirpaths = new List<string>(JSONHandler.BuildAllSubdirPathsWithoutFirst(tagDirectory.Name));

            RecursiveDirAdd(this, subdirpaths, tagDirectory);
        }

        private void RecursiveDirAdd(TagDirectory DirToAddInto, List<string> paths, TagDirectory DirToAdd)
        {
            if (DirToAddInto.SubDirectories != null)
            {
                if (paths.Count > 2 && DirToAddInto.SubDirectories.Exists(x => x.Name == paths[1]))
                {
                    paths.RemoveAt(0);
                    RecursiveDirAdd(DirToAddInto.SubDirectories.Find(x => x.Name == paths[1]), paths, DirToAdd);
                }

                else
                {
                    DirToAddInto.SubDirectories.Add(DirToAdd);
                }
            }

            else
            {
                DirToAddInto.SubDirectories = new List<TagDirectory>();
                RecursiveDirAdd(DirToAddInto, paths, DirToAdd);
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
}