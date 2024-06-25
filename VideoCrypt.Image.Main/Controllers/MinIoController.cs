using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace VideoCrypt.Image.Main.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinIoController : ControllerBase
{
    private readonly IMinioClient _minioClient;

    public MinIoController(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    [HttpGet("get-url")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUrl(string bucketID)
    {
        var presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucketID)
            .WithObject("imagesbacket")
            .WithExpiry(3000)); // You need to provide an object name

        return Ok(presignedUrl);
    }
}