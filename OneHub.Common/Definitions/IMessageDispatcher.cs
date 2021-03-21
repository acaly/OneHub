using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public interface IMessageDispatcher
    {
        bool TryGetSingleDispatchId(Type type, out (string key, string value) val);

        //Generate a piece of code to jump to retLabel if the multi-id check fails.
        //Return the priority (lower value will be checked first).
        int EmitRecvMultiIdCheck(Type type, ILGeneratorEnv il, Label retLabel, LocalBuilder jsonDocument);

        //Generate a piece of code returning a Task (for request and for event).
        //Response does not need to implement this method and will be handled by request's dispatcher.
        //The Task returned will not be awaitted.
        //msgBuffer is disposed by the callee.
        //For events, this method must also define event add and remove methods.
        //protocol parameter is only used for request dispatcher (impl side). For events, protocol is null.
        void EmitRecvAction(Type type, ILGeneratorEnv il, LocalBuilder protocol, LocalBuilder msgBuffer);

        //Generate a piece of code returning a Task (for event) or Task<TResponse> (for request).
        //The Task returned for the event will not be awaitted.
        //The Task returned for the request will be awaitted by the API caller.
        void EmitSendAction(Type type, ILGeneratorEnv il, LocalBuilder connection, LocalBuilder data);

        //This class is not a perfect abstraction of request and event dispatchers. See protocol parameter
        //in EmitRecvAction. Also there are many common codes in the dispatchers. Need improvements.
    }
}
