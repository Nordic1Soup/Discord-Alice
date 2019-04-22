using Newtonsoft.Json;

namespace mkdiscordbot
{
    internal class ServerSetting
    {
        public static ServerSetting GetInfo(string Path)
        {
            string json = System.IO.File.ReadAllText(Path);
            return JsonConvert.DeserializeObject<ServerSetting>(json);
        }

        public ulong ServerID { get; set; }
        public Informationchannel[] InformationChannels { get; set; }
        public Role[] Roles { get; set; }
        public MusicChannel[] MusicChannels { get; set; }
        public string[] InformationLanguage { get; set; }
        public ulong[] AdminIds { get; set; }
        public ulong GeneralId { get; set; }
        public Nsfwpolice NsfwPolice { get; set; }
        public bool StartMessage { get; set; }
        public bool ExitMessage { get; set; }
        public bool WelcomeMessage { get; set; }

        public string OpenWeatherMapAPIKey { get; set; }
        public string GASTranslater { get; set; }

        public class MusicChannel
        {
            public ulong textid { get; set; }
            public ulong vcid { get; set; }
        }

        public class Nsfwpolice
        {
            public bool enabled { get; set; }
            public bool delete { get; set; }
            public bool save { get; set; }
            public bool count { get; set; }
        }

        public class Informationchannel
        {
            public ulong id { get; set; }
            public string lang { get; set; }
        }

        public class Role
        {
            public ulong id { get; set; }
            public string lang { get; set; }
        }
    }
}