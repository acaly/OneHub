using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneBot11.API;
using OneHub.Common.Protocols.OneBot11.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11
{
    public interface IOneBot11
    {
        Task<SendPrivateMsg.Response> SendPrivateMsgAsync(SendPrivateMsg args);
        Task<SendGroupMsg.Response> SendGroupMsgAsync(SendGroupMsg args);
        Task<SendMsg.Response> SendMsgAsync(SendMsg args);
        Task<DeleteMsg.Response> DeleteMsgAsync(DeleteMsg args);
        Task<GetMsg.Response> GetMsgAsync(GetMsg args);
        Task<GetForwardMsg.Response> GetForwardMsgAsync(GetForwardMsg args);
        Task<SendLike.Response> SendLikeAsync(SendLike args);
        Task<SetGroupKick.Response> SetGroupKickAsync(SetGroupKick args);
        Task<SetGroupBan.Response> SetGroupBanAsync(SetGroupBan args);
        Task<SetGroupAnonymousBan.Response> SetGroupAnonymousBanAsync(SetGroupAnonymousBan args);
        Task<SetGroupWholeBan.Response> SetGroupWholeBanAsync(SetGroupWholeBan args);
        Task<SetGroupAdmin.Response> SetGroupAdminAsync(SetGroupAdmin args);
        Task<SetGroupAnonymous.Response> SetGroupAnonymousAsync(SetGroupAnonymous args);
        Task<SetGroupCard.Response> SetGroupCardAsync(SetGroupCard args);
        Task<SetGroupName.Response> SetGroupNameAsync(SetGroupName args);
        Task<SetGroupLeave.Response> SetGroupLeaveAsync(SetGroupLeave args);
        Task<SetGroupSpecialTitle.Response> SetGroupSpecialTitleAsync(SetGroupSpecialTitle args);
        Task<SetFriendAddRequest.Response> SetFriendAddRequestAsync(SetFriendAddRequest args);
        Task<SetGroupAddRequest.Response> SetGroupAddRequestAsync(SetGroupAddRequest args);
        Task<GetLoginInfo.Response> GetLoginInfoAsync(GetLoginInfo args);
        Task<GetStrangerInfo.Response> GetStrangerInfoAsync(GetStrangerInfo args);
        Task<GetFriendList.Response> GetFriendListAsync(GetFriendList args);
        Task<GetGroupInfo.Response> GetGroupInfoAsync(GetGroupInfo args);
        Task<GetGroupList.Response> GetGroupListAsync(GetGroupList args);
        Task<GetGroupMemberInfo.Response> GetGroupMemberInfoAsync(GetGroupMemberInfo args);
        Task<GetGroupMemberList.Response> GetGroupMemberListAsync(GetGroupMemberList args);
        Task<GetGroupHonorInfo.Response> GetGroupHonorInfoAsync(GetGroupHonorInfo args);
        Task<GetCookies.Response> GetCookiesAsync(GetCookies args);
        Task<GetCsrfToken.Response> GetCsrfTokenAsync(GetCsrfToken args);
        Task<GetCredentials.Response> GetCredentialsAsync(GetCredentials args);
        Task<GetRecord.Response> GetRecordAsync(GetRecord args);
        Task<GetImage.Response> GetImageAsync(GetImage args);
        Task<CanSendImage.Response> CanSendImageAsync(CanSendImage args);
        Task<CanSendRecord.Response> CanSendRecordAsync(CanSendRecord args);
        Task<GetStatus.Response> GetStatusAsync(GetStatus args);
        Task<GetVersionInfo.Response> GetVersionInfoAsync(GetVersionInfo args);
        Task<SetRestart.Response> SetRestartAsync(SetRestart args);
        Task<CleanCache.Response> CleanCacheAsync(CleanCache args);

        event AsyncEventHandler<PrivateMessage> PrivateMessage;
        event AsyncEventHandler<GroupMessage> GroupMessage;
        event AsyncEventHandler<GroupUpload> GroupUpload;
        event AsyncEventHandler<GroupAdmin> GroupAdmin;
        event AsyncEventHandler<GroupDecrease> GroupDecrease;
        event AsyncEventHandler<GroupIncrease> GroupIncrease;
        event AsyncEventHandler<GroupBan> GroupBan;
        event AsyncEventHandler<FriendAdd> FriendAdd;
        event AsyncEventHandler<GroupRecall> GroupRecall;
        event AsyncEventHandler<FriendRecall> FriendRecall;
        event AsyncEventHandler<PokeNotify> PokeNotify;
        event AsyncEventHandler<LuckyKingNotity> LuckyKingNotity;
        event AsyncEventHandler<HonorNotify> HonorNotify;
        event AsyncEventHandler<FriendRequest> FriendRequest;
        event AsyncEventHandler<GroupRequest> GroupRequest;
        event AsyncEventHandler<Lifecycle> Lifecycle;
        event AsyncEventHandler<Heartbeat> Heartbeat;
    }
}
