using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Seeka.Managers.Access.Resumes;

namespace Seeka.ResourceAccess.Accessors;

/// <summary>R2 through its S3-compatible API, with an API token scoped to one bucket.</summary>
internal sealed class R2ResumeFileAccess(IAmazonS3 s3, R2Settings settings, ILogger<R2ResumeFileAccess> logger)
    : IResumeFileAccess
{
    public static IAmazonS3 CreateClient(R2Settings settings) =>
        new AmazonS3Client(
            new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL = $"https://{settings.AccountId}.r2.cloudflarestorage.com",
                AuthenticationRegion = "auto",
                ForcePathStyle = true,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            });

    public async Task SaveAsync(Guid workspaceId, Guid resumeId, byte[] content, CancellationToken cancellationToken)
    {
        using var body = new MemoryStream(content);
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = settings.Bucket,
            Key = ResumeStoragePolicy.Key(workspaceId, resumeId),
            InputStream = body,
            ContentType = "application/pdf",
            DisablePayloadSigning = true
        }, cancellationToken);
    }

    public async Task<Stream?> OpenAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await s3.GetObjectAsync(
                settings.Bucket,
                ResumeStoragePolicy.Key(workspaceId, resumeId),
                cancellationToken);
            var copy = new MemoryStream();
            await response.ResponseStream.CopyToAsync(copy, cancellationToken);
            copy.Position = 0;
            return copy;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken)
    {
        try
        {
            await s3.DeleteObjectAsync(settings.Bucket, ResumeStoragePolicy.Key(workspaceId, resumeId), cancellationToken);
        }
        catch (Exception exception) when (exception is AmazonServiceException or HttpRequestException)
        {
            logger.LogWarning(exception, "Could not delete résumé {ResumeId} from R2.", resumeId);
        }
    }
}
