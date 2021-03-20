using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs eventArgs);
}
