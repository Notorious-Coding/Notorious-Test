using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Exceptions
{
    public class InfrastructureNotFoundException : Exception
    {
        public InfrastructureNotFoundException()
        {
        }

        public InfrastructureNotFoundException(string? message) : base(message)
        {
        }

        public InfrastructureNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
