using CloudinaryDotNet.Actions;

namespace API.Interfaces
{
    public interface IphotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}