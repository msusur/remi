using System;
using System.IO;
using System.Linq;
using ReMi.Common.WebApi;

namespace ReMi.Api.Insfrastructure
{
    public class FileStorage : IFileStorage, IAppDataFileStorage
    {
        private readonly string _rootPath;

        public FileStorage(string rootPath)
        {
            _rootPath = rootPath;
        }

        public string[] GetFiles(string relativePath = null)
        {
            var dirPath = ConvertRelativePathToAbsolute(relativePath ?? string.Empty);

            return Directory.GetFiles(dirPath).Select(Path.GetFileName).ToArray();
        }

        public byte[] GetFileContent(string fileName, string relativePath = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            var filePath = ConvertRelativePathToAbsolute(relativePath ?? string.Empty, fileName);

            if (!File.Exists(filePath))
                throw new Exception(string.Format("File {0} not exists!", fileName));

            return File.ReadAllBytes(filePath);
        }

        public void DeleteFile(string fileName, string relativePath = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            var filePath = ConvertRelativePathToAbsolute(relativePath ?? string.Empty, fileName);

            if (!File.Exists(filePath))
                throw new Exception(string.Format("File {0} not exists!", fileName));

            File.Delete(filePath);
        }

        private string ConvertRelativePathToAbsolute(string relativePath, string fileName = null)
        {
            return Path.Combine(_rootPath, relativePath, fileName ?? string.Empty);

        }
    }
}
