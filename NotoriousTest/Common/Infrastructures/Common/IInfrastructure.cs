using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Infrastructures.Common
{
    public interface IInfrastructure
    {
        /// <summary>
        /// Allows defining the order in which the infrastructure will be executed within the environment.
        /// </summary>
        int? Order { get; }

        /// <summary>
        /// AutoReset is a flag to reset the infrastructure after each test. Default is true.
        /// </summary>
        bool AutoReset { get; set; }

        /// <summary>
        /// ContextId is an identfier of the current infrastructure context.
        /// In standalone mode : ContextId is a unique identifier scoped for the infrastructure.
        /// Withing an environment : ContextId is the same as the environmentId.
        /// </summary>
        public Guid ContextId { get; set; }
    }
}
