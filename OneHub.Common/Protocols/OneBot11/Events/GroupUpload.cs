using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [OneBot11Event]
    public sealed class GroupUpload : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupUpload";

        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public UploadInfo File { get; set; }

        public sealed class UploadInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public ulong Size { get; set; }
            public ulong Busid { get; set; }
        }
    }
}
