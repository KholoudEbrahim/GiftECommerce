namespace CategoryService.Contracts;


public interface IFileStorageService
{
    string GetFullUrl(string? relativePath);

    // New Methods
    Task<string> SaveFileAsync(IFormFile? file, string folderName);
    void DeleteFile(string? relativePath);
}