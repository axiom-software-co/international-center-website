namespace SharedPlatform.Features.ContentManagement.Abstractions;

/// <summary>
/// Content hashing service for immutable content versioning
/// Provides SHA256 hashing for content integrity and cache-busting
/// </summary>
public interface IContentHashService
{
    /// <summary>
    /// Generate SHA256 hash of content for immutability and versioning
    /// </summary>
    /// <param name="content">Content to hash</param>
    /// <returns>SHA256 hash in hexadecimal format</returns>
    string GenerateContentHash(string content);

    /// <summary>
    /// Generate SHA256 hash from content bytes
    /// </summary>
    /// <param name="contentBytes">Content bytes to hash</param>
    /// <returns>SHA256 hash in hexadecimal format</returns>
    string GenerateContentHash(byte[] contentBytes);

    /// <summary>
    /// Verify content matches expected hash
    /// </summary>
    /// <param name="content">Content to verify</param>
    /// <param name="expectedHash">Expected SHA256 hash</param>
    /// <returns>True if content matches hash</returns>
    bool VerifyContentHash(string content, string expectedHash);

    /// <summary>
    /// Verify content bytes match expected hash
    /// </summary>
    /// <param name="contentBytes">Content bytes to verify</param>
    /// <param name="expectedHash">Expected SHA256 hash</param>
    /// <returns>True if content matches hash</returns>
    bool VerifyContentHash(byte[] contentBytes, string expectedHash);
}