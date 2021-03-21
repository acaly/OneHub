using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public interface IUseBinaryMessageDecoderAttribute
    {
        public Type DecoderType { get; }
    }
}
