using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary.Error;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class ImageService
    {
        private readonly Configuration configuration;
        private readonly ConfigurationService configurationService;

        public ImageService()
        {
            configurationService = new ConfigurationService();
            configuration = configurationService.SystemConfiguration;
        }

        public Guid StoreRoastImage(string fileLocation)
        {
            var fileId = Guid.NewGuid();
            try
            {
                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                db.FileStorage.Upload(fileId.ToString(), fileLocation);
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(ImageService)} failed when attempting to save the image file {fileLocation}", ex);
                fileId = Guid.Empty;
            }

            return fileId;
        }

        public MemoryStream Load(Guid fileId)
        {
            try
            {
                if (fileId == Guid.Empty)
                    return null;

                using var db = new LiteDatabase(configuration.LogFileDatabaseDirectory);
                using (var file = db.FileStorage.OpenRead(fileId.ToString()))
                {
                    var stream = new MemoryStream();
                    file.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Database, $"Class {typeof(ImageService)} failed when attempting to save the image file {fileId}", ex);
                return null;
            }
        }
    }
}