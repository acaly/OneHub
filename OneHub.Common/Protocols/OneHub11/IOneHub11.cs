using OneHub.Common.Protocols.OneHub11.API;
using OneHub.Common.Protocols.OneHub11.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11
{
    public interface IOneHub11
    {
        Task<SendMsg.Response> SendMsgAsync(SendMsg args);
        Task<DeleteMsg.Response> DeleteMsgAsync(DeleteMsg args);
        Task<GetMsg.Response> GetMsgAsync(GetMsg args);
        Task<GetHistory.Response> GetHistoryAsync(GetHistory args);

        Task<UploadBlob.Response> UploadBlobAsync(UploadBlob args);
        Task<DownloadBlob.Response> DownloadBlobAsync(DownloadBlob args);

        Task<GetFrientList.Response> GetFrientListAsync(GetFrientList args);
        Task<GetGroupList.Response> GetGroupListAsync(GetGroupList args);
        Task<GetChannelList.Response> GetChannelListAsync(GetChannelList args);
        Task<GetUserInfo.Response> GetUserInfoAsync(GetUserInfo args);
        Task<SetUserInfo.Response> SetUserInfoAsync(SetUserInfo args);
        Task<GetGroupInfo.Response> GetGroupInfoAsync(GetGroupInfo args);
        Task<SetGroupInfo.Response> SetGroupInfoAsync(SetGroupInfo args);
        Task<GetChannelInfo.Response> GetChannelInfoAsync(GetChannelInfo args);
        Task<SetChannelInfo.Response> SetChannelInfoAsync(SetChannelInfo args);
        Task<GetGroupMemberList.Response> GetGroupMemberListAsync(GetGroupMemberList args);
        Task<GetGroupMemberInfo.Response> GetGroupMemberInfoAsync(GetGroupMemberInfo args);
        Task<SetGroupMemberInfo.Response> SetGroupMemberInfoAsync(SetGroupMemberInfo args);

        Task<SetNotif.Response> SetNotifAsync(SetNotif args);
        Task<GetVersionInfo.Response> GetVersionInfoAsync(GetVersionInfo args);
        Task<GetApiCapability.Response> GetApiCapabilityAsync(GetApiCapability args);
        Task<Heartbeat.Response> HeartbeatAsync(Heartbeat args);

        event AsyncEventHandler<ChannelInfoChanged> ChannelInfoChanged;
        event AsyncEventHandler<GroupInfoChanged> GroupInfoChanged;
        event AsyncEventHandler<GroupMemberChanged> GroupMemberChanged;
        event AsyncEventHandler<MessageReceived> MessageReceived;
        event AsyncEventHandler<UserInfoChanged> UserInfoChanged;
    }
}
