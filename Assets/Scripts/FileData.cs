﻿using System;

namespace EBookReader
{
    [Serializable]
    public class FileData
    {
        public enum FileType { Book, UserFile }

        public string Name;
        public string Path;
        public string ImagePath;
        public FileType Type;

        public int CurCharIndex;

        public FileData(string name, string path, string imagePath, FileType type)
        {
            Name = name;
            Path = path;
            ImagePath = imagePath;
            Type = type;
        }
    }
}
