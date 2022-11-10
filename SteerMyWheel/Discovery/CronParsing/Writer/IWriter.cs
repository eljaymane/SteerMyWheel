using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Writer
{
    public interface IWriter<T>
    {
        Task WriteAsync(T value);
    }
}
