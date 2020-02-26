using System;
using System.Collections.Generic;
using System.Text;

namespace SCBot.Parsers
{
    public interface IFileWriter<T>
    {
        void Write(string filePath, IList<T> items);
    }
}