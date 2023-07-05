using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFanout.Adapters
{
    public interface IDataInput<in TValue>
    {
        void Push(TValue value);
    }
}
