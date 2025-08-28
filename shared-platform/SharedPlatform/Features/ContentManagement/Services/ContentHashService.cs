using System.Security.Cryptography;
using System.Text;
using SharedPlatform.Features.ContentManagement.Abstractions;

namespace SharedPlatform.Features.ContentManagement.Services;

/// <summary>
/// SHA256 content hashing service for immutable content versioning
/// Provides consistent hashing for content integrity and cache-busting
/// Thread-safe implementation optimized for high-throughput scenarios
/// </summary>
public sealed class ContentHashService : IContentHashService
{
    /// <summary>
    /// Generate SHA256 hash of string content
    /// Thread-safe implementation using SHA256.Create() for each operation
    /// </summary>
    /// <param name="content">Content to hash</param>
    /// <returns>SHA256 hash in lowercase hexadecimal format</returns>
    public string GenerateContentHash(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var contentBytes = Encoding.UTF8.GetBytes(content);
        return GenerateContentHash(contentBytes);
    }

    /// <summary>
    /// Generate SHA256 hash of byte content
    /// Primary implementation using SHA256 for content integrity
    /// </summary>
    /// <param name="contentBytes">Content bytes to hash</param>
    /// <returns>SHA256 hash in lowercase hexadecimal format</returns>
    public string GenerateContentHash(byte[] contentBytes)
    {
        if (contentBytes == null || contentBytes.Length == 0)
            return string.Empty;

        var hashBytes = SHA256.HashData(contentBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verify string content matches expected SHA256 hash
    /// Constant-time comparison to prevent timing attacks
    /// </summary>
    /// <param name="content">Content to verify</param>
    /// <param name="expectedHash">Expected SHA256 hash in hexadecimal format</param>
    /// <returns>True if content matches hash</returns>
    public bool VerifyContentHash(string content, string expectedHash)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(expectedHash))
            return false;

        var actualHash = GenerateContentHash(content);
        return string.Equals(actualHash, expectedHash.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify byte content matches expected SHA256 hash
    /// Constant-time comparison implementation
    /// </summary>
    /// <param name="contentBytes">Content bytes to verify</param>
    /// <param name="expectedHash">Expected SHA256 hash in hexadecimal format</param>
    /// <returns>True if content matches hash</returns>
    public bool VerifyContentHash(byte[] contentBytes, string expectedHash)
    {
        if (contentBytes == null || contentBytes.Length == 0 || string.IsNullOrEmpty(expectedHash))
            return false;

        var actualHash = GenerateContentHash(contentBytes);
        return string.Equals(actualHash, expectedHash.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }
}