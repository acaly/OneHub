using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneHub11.Objects;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [JsonConverter(typeof(Enum32JsonConverter<RequestContentType>))]
    public enum RequestContentType
    {
        None,
        Abstract,
        Default,
        Fulltext,
    }

    [JsonConverter(typeof(Enum32JsonConverter<RequestChildrenType>))]
    public enum RequestChildrenType
    {
        None,
        Default,
        Max,
    }

    [OneHub11ApiRequest]
    public sealed class GetMsg
    {
        public string MessageId { get; set; }
        public RequestContentType RequestContent { get; set; }
        public RequestChildrenType RequestChildren { get; set; }

        public sealed class Response
        {
            public MessageInfo Message { get; set; }
            public List<MessageInfo> Children { get; set; }
        }
    }
}
