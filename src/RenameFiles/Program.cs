using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RenameFiles
{
    class Program
    {
        private static int filesRenamedCount;

        static void Main(string[] args)
        {
            string dir = GetFolder(args);
            var regex = new Regex("^([0-9]{1,2}\\w{0,1}-{0,1}\\.{0,1}[0-9]{0,2}\\w{0,1})");
            var filesToRename = GetFilesToRename(dir, regex, "*.mp3");
            var notRenamedFiles = RenameFiles(regex, filesToRename);
            ReportResults(notRenamedFiles, filesRenamedCount);
        }

        private static string GetFolder(string[] args)
        {
            var dir = Directory.GetCurrentDirectory();
            if (args.Length > 0)
                dir = args[0];
            return dir;
        }

        private static IEnumerable<string> GetFilesToRename(string dir, Regex regex, string filter)
        {
            var files = Directory.GetFiles(dir, filter);
            var filesToRename = new List<string>();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (regex.IsMatch(fileInfo.Name))
                    filesToRename.Add(file);
            }
            return filesToRename;
        }

        private static IEnumerable<string> RenameFiles(Regex regex, IEnumerable<string> filesToRename)
        {
            filesRenamedCount = 0;
            var notRenamesFiles = new List<string>();
            foreach (var file in filesToRename)
            {
                TryToRename(regex, notRenamesFiles, file);
            }
            return notRenamesFiles;
        }

        private static void TryToRename(Regex regex, List<string> notRenamesFiles, string file)
        {
            try
            {
                RenameFile(file, regex);
                filesRenamedCount++;
            }
            catch (IOException)
            {
                // if cannot rename leave as it is
                notRenamesFiles.Add(file);
            }
        }

        private static void ReportResults(IEnumerable<string> possibleDuplicates, int filesRenamedCount)
        {
            if (possibleDuplicates.Count() > 0)
            {
                Console.WriteLine("Could not rename these files, possible name collision:");
                Console.WriteLine(string.Join(Environment.NewLine, possibleDuplicates));
            }
            Console.WriteLine("Successfully renamed {0} files", filesRenamedCount);
        }

        private static void RenameFile(string file, Regex regex)
        {
            var fileInfo = new FileInfo(file);
            var newName = regex.Replace(fileInfo.Name, string.Empty);
            newName = newName.TrimStart(new char[] { ' ', '-', '.' });
            var filePath = fileInfo.DirectoryName + newName;
            File.Move(file, filePath);
        }
    }
}
