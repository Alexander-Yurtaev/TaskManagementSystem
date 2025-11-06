namespace TMS.Common.Services;

public interface IFileService
{
    string BasePath { get; set; }

    void WriteFile(string path, Action<FileStream> action);
    void WriteFile(string path, string content);
    Stream GetFile(string filePath);
}