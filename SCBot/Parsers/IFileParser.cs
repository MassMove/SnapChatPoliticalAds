using System.Collections.Generic;

namespace SCBot.Parsers
{
    public interface IFileParser<T> where T : class, new()
    {
        List<T> Parse(string filePath);
    }
}