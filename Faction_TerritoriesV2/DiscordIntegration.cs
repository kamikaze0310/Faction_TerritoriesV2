using ProtoBuf;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Utils;

namespace Faction_TerritoriesV2
{
    [ProtoContract]
    public class ModMessage
    {
        [ProtoMember(1)] public long RoleID;
        [ProtoMember(2)] public string RoleName;
        [ProtoMember(3)] public string Message;
        [ProtoMember(4)] public string Author;
        [ProtoMember(5)] public bool BroadCastToDiscordOnly;
        [ProtoMember(6)] public VRageMath.Color Color;

        public ModMessage(long DiscordRole, string DiscordRoleName, string MessageTxt, string MsgAuthor, VRageMath.Color color, bool BrodcastDiscordOnly = false, string ChannelId = null)
        {
            RoleID = DiscordRole;
            RoleName = DiscordRoleName;
            Author = MsgAuthor;
            Color = color;
            BroadCastToDiscordOnly = BrodcastDiscordOnly;
            Message = MessageTxt;

            MyVisualScriptLogicProvider.SendChatMessageColored($"<@&{DiscordRole}> [{RoleName}]: {MessageTxt}", color, MsgAuthor, 0, "Red");

            if (Session.Instance.IsNexusInstalled)
            {
                //MyLog.Default.WriteLineAndConsole($"NexusAPI: Chat Message Init");
                NexusAPI.SendMessageToDiscord($"<@&{DiscordRole}> [{RoleName}]: {MessageTxt}");
                NexusComms.SendChatAllServers(this);
            }

            //var sendData = MyAPIGateway.Utilities.SerializeToBinary(this);
            //MyAPIGateway.Multiplayer.SendMessageToServer(2910, sendData);
        }

        public ModMessage() { }
    }


}
