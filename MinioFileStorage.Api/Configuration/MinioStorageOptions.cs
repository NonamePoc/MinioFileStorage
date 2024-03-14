namespace MinioFileStorage.Api.Configuration;

public class MinioStorageOptions
{
    public string AccessKey { get; set; }

    public string SecretAccessKey { get; set; }

    public string ServiceUrl { get; set; }

    public string BucketName { get; set; }
        
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);
}