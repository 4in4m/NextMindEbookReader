using System;

namespace EBookReader
{
    [Serializable]
    public class FileData
    {
        public enum FileType { Book, UserFile, PdfFile }

        public string Name;
        public string Path;
        public string CoverImagePath;
        public FileType Type;

        public int CurCharIndex;

        public string[] ImagesPaths;

        public FileData(string name, string path, string imagePath, FileType type, string[] imagesPaths = null)
        {
            Name = name;
            Path = path;
            CoverImagePath = imagePath;
            Type = type;
            ImagesPaths = imagesPaths;
        }
    }
}
