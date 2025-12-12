using CategoryService.Contracts;

namespace CategoryService.shared.Services; 

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public LocalFileStorageService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
    {
        _httpContextAccessor = httpContextAccessor;
        _webHostEnvironment = webHostEnvironment;
    }

    public string GetFullUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;

        // SAFETY CHECK: If it's already a full URL (e.g. from seed data), don't touch it.
        if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return relativePath;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return relativePath;

        var baseUrl = $"{request.Scheme}://{request.Host}";

        // Ensure we don't double slash and standardize separators
        var cleanPath = relativePath.Replace("\\", "/").TrimStart('/');

        return $"{baseUrl}/{cleanPath}";
    }

    public async Task<string> SaveFileAsync(IFormFile? file, string folderName)
    {
        if (file is null || file.Length == 0) return string.Empty;

        // 1. Define Path: wwwroot/images/{folderName}
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // 2. Generate Unique Filename
        string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 3. Save to Disk
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // 4. Return Relative Path for DB (e.g., "images/categories/abc.jpg")
        return Path.Combine("images", folderName, uniqueFileName).Replace("\\", "/");
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception)
        {
            // Log error or ignore - usually we don't want to crash if file delete fails
        }
    }
}