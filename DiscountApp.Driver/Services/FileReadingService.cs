using DiscountApp.Driver.Core;

namespace DiscountApp.Driver.Services;

/// <summary>
/// File reading wrapped in functional Result type.
/// </summary>
public interface IFileReadingService
{
    /// <summary>
    /// Read all lines.
    /// </summary>
    /// <param name="filename">Filename</param>
    /// <returns>Read lines or error</returns>
    Result<string[]> ReadAllLines(string filename);
}

/// <inheritdoc/>
public class FileReadingService : IFileReadingService
{
    /// <inheritdoc/>
    public Result<string[]> ReadAllLines(string filename)
    {
        try
        {
            return File.ReadAllLines(filename);
        }
        catch(Exception ex)
        {
            return ex.Message;
        }
    }
}