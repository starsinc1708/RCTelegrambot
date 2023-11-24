using RCTelegramBot.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RCTelegramBot.Bot
{
    internal class FileManager
    {
        public string CurrentFolder { get; set; }
        public string PreviousFolder { get; set; }
        public string ProgramFolder { get; set; }
        public string RootFolder { get; set; }

        public List<string> Files { get; set; }
        public List<string> Directories { get; set; }

        public int Page { get; set; }
        public int MaxPages { get; set; }
        public int ContentCount { get; set; }
        public string CurrentFile { get; private set; }

        public int DisplayedListLength = 20;

        public FileManager(string programFolder)
        {
            ProgramFolder = programFolder;
            CurrentFolder = programFolder;
            RootFolder = Path.GetPathRoot(CurrentFolder);
            Page = 1;
        }

        internal void ResetFolderPath()
        {
            PreviousFolder = CurrentFolder;
            CurrentFolder = ProgramFolder;
            RootFolder = Path.GetPathRoot(CurrentFolder);
            Page = 1;
        }

        internal string CurrentFolderContent(int page = 1)
        {
            Page = page;
            Files = Directory.GetFiles(CurrentFolder).OfType<string>().ToList();
            Directories = Directory.GetDirectories(CurrentFolder).OfType<string>().ToList();
            ContentCount = Files.Count + Directories.Count;
            MaxPages = (int)Math.Ceiling((double)ContentCount / DisplayedListLength);
            try
            {
                string result = $"Содержимое текущей папки\n[{CurrentFolder}]\n";
                for (int i = (Page - 1) * DisplayedListLength; i < ContentCount; i++)
                {
                    if (i == Page * DisplayedListLength) break;

                    if (Directories.Count > i)
                    {
                        string generatedCommand = $"/folder_{i}";
                        result += $"{Smile.FOLDER} {Path.GetFileName(Directories[i])}\t|\t{generatedCommand}\n";
                    }
                    else
                    {
                        string generatedCommand = $"/file_{i - Directories.Count}";
                        result += $"{Smile.FILE} {Path.GetFileName(Files[i - Directories.Count])}\t|\t{generatedCommand}\n";
                    }
                }

                result += $"\n\t\t\t\tстраница {Page} из {(ContentCount > 0 ? MaxPages : 1)}";

                return result;
            }
            catch (Exception ex)
            {
                return "Произошла ошибка при получении содержимого текущей директории: " + ex.Message;
            }
        }

        public void MoveToRootFolder()
        {
            PreviousFolder = CurrentFolder;
            CurrentFolder = RootFolder;
            Page = 1;
        }

        public List<string> GetAllRoots()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            List<string> driveLetters = new List<string>();

            foreach (DriveInfo drive in drives)
            {
                driveLetters.Add(drive.Name);
            }

            return driveLetters;
        }

        internal void MoveUp()
        {
            if (Directory.GetParent(CurrentFolder) == null) return;
            PreviousFolder = CurrentFolder;
            CurrentFolder = Directory.GetParent(CurrentFolder).FullName;
            Page = 1;
        }

        internal void GoToPath(string filepath)
        {
            if (Path.GetPathRoot(CurrentFolder) != Path.GetPathRoot(filepath)) RootFolder = Path.GetPathRoot(filepath);
            PreviousFolder = CurrentFolder;
            CurrentFolder = filepath;
            Page = 1;
        }

        internal void MovetoFolder(int folderNumber)
        {
            if (folderNumber < 0 && folderNumber > Directories.Count) return;
            PreviousFolder = CurrentFolder;
            CurrentFolder = Directories[folderNumber];
        }

        internal string ShowFileInfo(int fileNumber)
        {
            CurrentFile = Files[fileNumber];
            string result = $"{Smile.FILE} Имя файла: {Path.GetFileName(Files[fileNumber])}\n";
            result += $"Путь: {Files[fileNumber]}\n";
            double length = new System.IO.FileInfo(Files[fileNumber]).Length / 1024.0;
            if (length > 1024)
            {
                length /= 1024.0;
                result += $"Размер: {Math.Round(length, 2)} MB\n";
            }
            else
            {
                result += $"Размер: {Math.Round(length, 2)} KB\n";
            }
            return result;
        }

        internal void OpenFile()
        {
            Process.Start(CurrentFile);
        }
    }
}