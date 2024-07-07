using Dapper;
using VideoCrypt.Image.Data.Migrations;

namespace VideoCrypt.Image.Server.Dapper;

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
    }
}