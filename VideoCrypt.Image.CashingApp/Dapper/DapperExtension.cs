using Dapper;
using VideoCrypt.Image.Data.Migrations;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Server.Dapper
{
    public static class DapperExtensions
    {
        public static void ConfigureTypeMappings()
        {
            SqlMapper.SetTypeMap(
                typeof(ImageMetadata),
                new CustomPropertyTypeMap(
                    typeof(ImageMetadata),
                    (type, columnName) =>
                    {
                        return type.GetProperty(
                            columnName.Replace("file_name", "FileName")
                                .Replace("cached_file_path", "CachedFilePath")
                                .Replace("created_at", "CreatedAt")
                                .Replace("url", "Url")
                                .Replace("id", "Id"));
                    }));

            SqlMapper.SetTypeMap(
                typeof(ApiKey),
                new CustomPropertyTypeMap(
                    typeof(ApiKey),
                    (type, columnName) =>
                    {
                        return type.GetProperty(
                            columnName.Replace("key", "Key")
                                .Replace("name", "Name")
                                .Replace("description", "Description")
                                .Replace("created_at", "CreatedAt")
                                .Replace("expire_at", "ExpireAt")
                                .Replace("id", "Id"));
                    }));
        }
    }
}