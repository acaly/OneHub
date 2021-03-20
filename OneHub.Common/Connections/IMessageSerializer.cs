using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    public interface IMessageSerializer<T>
    {
        void Serialize(MessageBuffer messageBuffer, T obj);
        T Deserialize(MessageBuffer messageBuffer);
    }
}
