using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Service
{
    public class PhotoService : IphotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config) 
        // este parametro inyecta las configuraciones de cloudinary que se encuentran
        // en appsettings.json
        {
            var acc = new Account( // se recibe todas las configuraciones de cloudinary
                config.Value.CloudName, 
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc); // el objeto cloundary recibe la configuración
            // es decir CloudName, ApiKey, ApiSecret
        }
        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file) // representa el archivo
        {
            var uploadResult = new ImageUploadResult(); // contiene los parametros para subir
            // imagenes en cloudinary, sea tamaño, transformaciones, posición de la imagen
            
            if(file.Length > 0) //  si existe el archivo
            {
                using var stream = file.OpenReadStream(); // establece un punto de lectura para
                // acceder a los archivos de la interfaz IFormFile
                var uploadParams = new ImageUploadParams 
                {
                    File = new FileDescription(file.FileName, stream), // proporciona el nombre
                    // del archivo y mediante la variable stream el contenido
                    Transformation = new Transformation().Height(500)
                    .Width(500).Crop("fill").Gravity("face"),
                    // Height(Alto = 500px) Width(Ancho= 500px)
                    // Crop("fill") recorta("fill") recorta la imagen a 500px sin distorcionarla
                    // Gravity("face") indica que la transformación debe ser centrada en la cara
                    Folder = "da-net7" // se establece la carpeta donde se va a guardar las imagenes
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams); 
                // se sube el archivo a cloudinary
            }

            return uploadResult; // devuelve el resultado
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        // publicId representa el identificador publico de la imagen en cloudinary
        {
            var deleteParams = new DeletionParams(publicId); 
            // crea un DeletionParams con el identificador publico de la imagen en cloudinary

            return await _cloudinary.DestroyAsync(deleteParams); // elimina la imagen de acuerdo
            // con los parametros de DeletionParams.
        }
    }
}