using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mkdiscordbot
{
    class DiscordServerInformation
    {
        public DiscordServerInformation(string Path)
        {
            DPath = Path;
            UPath = System.IO.Path.Combine(DPath, "Users");
            Id = ulong.Parse(System.IO.Path.GetFileName(Path));
            S = ServerSetting.GetInfo(System.IO.Path.Combine(DPath,"serverconfig.json"));
            U = UserDictionary.Get(UPath);
        }

        public void Save()
        {
            UserDictionary.Save(UPath, U.Values.ToArray());
        }


        public ulong Id;
        public string DPath;
        public string UPath;
        public ServerSetting S;
        public Dictionary<ulong, UserDictionary> U;
        public SocketGuild Guild;
    }
}
