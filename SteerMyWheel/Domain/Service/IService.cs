using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Service
{
    public interface IService
    {
        void setLoggerFactory(ILoggerFactory loggerFactory);
    }
}
