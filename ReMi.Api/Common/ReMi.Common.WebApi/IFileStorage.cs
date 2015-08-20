namespace ReMi.Common.WebApi
{
    public interface IFileStorage
    {
        string[] GetFiles(string relativePath = null);

        byte[] GetFileContent(string fileName, string relativePath = null);

        void DeleteFile(string fileName, string relativePath = null);
    }
}
