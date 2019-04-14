using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using NYoutubeDL.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mkdiscordbot
{
    internal class Program
    {
        public static ServerSetting svSettings;
        public static Dictionary<string, LocaleDef> L;
        public static Dictionary<ulong, UserDictionary> U;

        private static string Token;
        public static DiscordSocketClient _client;
        public static SocketGuild Guild;

        private static bool LOCK = true;

        private static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(@"C:\Alice_dbgsv");

            if (!System.IO.File.Exists("token"))
            {
                Console.WriteLine("No Token");
                Environment.Exit(1);
            }
            Token = System.IO.File.ReadAllText("token");

            if (!System.IO.File.Exists("serverconfig.json"))
            {
                Console.WriteLine("No Server settings file");
                Environment.Exit(1);
            }
            if (!System.IO.Directory.Exists("Locale") || System.IO.Directory.GetFiles("Locale", "*-*.json").Length < 1)
            {
                Console.WriteLine("No Locale Files");
                Environment.Exit(1);
            }
            Console.WriteLine("Applying Server Settings");
            ServerSetting_Service.Init();
            Locale.InitLocaleInfo();

            if (!System.IO.Directory.Exists("Users"))
            {
                System.IO.Directory.CreateDirectory("Users");
            }
            UserDictionary_Service.Load();

            if (!System.IO.Directory.Exists("Alice_temporary_service_datas"))
            {
                System.IO.Directory.CreateDirectory("Alice_temporary_service_datas");
            }

            Console.WriteLine("Loading Alice Core System");
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.UserJoined += _client_UserJoined;
        }

        private async Task _client_UserJoined(SocketGuildUser arg)
        {
            string str = "";

            if (svSettings.WelcomeMessage)
            {
                foreach (string lang in svSettings.InformationLanguage)
                {
                    str += $"--- {lang} --------------------\n";
                    str += L[lang].Informations.welcomemsg.Replace("%name%", arg.Username) + "\n";
                }

                await ((ISocketMessageChannel)_client.GetChannel(svSettings.GeneralId)).SendMessageAsync(str);
            }
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            Guild = _client.GetGuild(svSettings.ServerID);

            Console.WriteLine("Type 'exit' to exit");
            while (true)
            {
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "exit":
                        LOCK = true;
                        UserDictionary_Service.Save();
                        Console.WriteLine("Userdata saved");
                        if (svSettings.ExitMessage)
                        {
                            foreach (ServerSetting.Informationchannel ich in svSettings.InformationChannels)
                            {
                                await ((ISocketMessageChannel)_client.GetChannel(ich.id))
                                    .SendMessageAsync(L[ich.lang].Informations.shutdown);
                            }
                        }
                        Environment.Exit(0);
                        break;

                    case "save":
                        LOCK = true;
                        UserDictionary_Service.Save();
                        Console.WriteLine("Userdata saved");
                        LOCK = false;
                        break;
                }
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine("[DISCORD SERVICE] says : " + log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine($"Alice connected to Discord service with {_client.CurrentUser}");

            if (svSettings.StartMessage)
            {
                foreach (ServerSetting.Informationchannel ich in svSettings.InformationChannels)
                {
                    await ((ISocketMessageChannel)_client.GetChannel(ich.id))
                        .SendMessageAsync(L[ich.lang].Informations.booted);
                }
            }

            LOCK = false;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            Task.Run(()=> CmdHandle(message));
        }

        private async Task CmdHandle(SocketMessage message)
        {
            if (LOCK) return;

            if (message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot)
                return;

            #region detect_lang

            string u_loc = "";

            foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
            {
                foreach (ServerSetting.Role srole in svSettings.Roles)
                {
                    if (role.Id == srole.id)
                    {
                        u_loc = srole.lang;
                        break;
                    }
                }
                if (u_loc != "") break;
            }

            if (u_loc == "") u_loc = "en-US";

            #endregion detect_lang

            #region TimeService

            if (Regex.IsMatch(message.Content, "^!set timezone (\\+|-)?[0-9]{1,2}:[0-9]{2}"))
            {
                string tzone = Regex.Replace(message.Content, "^!set timezone (\\+)?(?<tzone>(-)?[0-9]{1,2}:[0-9]{2})", "${tzone}");
                TimeSpan TimeZone = TimeSpan.Parse(tzone);
                DateTime Now = DateTime.UtcNow.Add(TimeZone);
                Console.WriteLine($"{message.Author.Username} wants to change Timezone to \"{tzone}\" (Current:{Now.ToShortTimeString()})");

                if (!U.ContainsKey(message.Author.Id))
                    U.Add(message.Author.Id, new UserDictionary(message.Author.Id));
                U[message.Author.Id].TimeZone = TimeZone;

                await message.Channel.SendMessageAsync(L[u_loc].UserSettingReplys.timezoneset);
            }

            if (message.Content == "!time")
            {
                await message.Channel.SendMessageAsync(GetUserTime(message.Author.Id, u_loc));
            }

            if (Regex.IsMatch(message.Content, "^!time <@[0-9]+>"))
            {
                ulong uid = ulong.Parse(Regex.Replace(message.Content, "^!time <@(?<tzone>[0-9]+)>", "${tzone}"));

                await message.Channel.SendMessageAsync(GetUserTime(uid, u_loc));
            }

            if (Regex.IsMatch(message.Content, @"^What time is it now\s?(\.|\?)?\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(GetUserTime(message.Author.Id, u_loc));

            if (Regex.IsMatch(message.Content, @"^(今|いま)、?(何時|なんじ)(。|？|\?)\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(GetUserTime(message.Author.Id, u_loc));

            #endregion TimeService

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("[ALICE CHATTING SUPPORTER] says: pong!");

            if (message.Content == "!langs")
            {
                string msg = "[ALICE LANGUAGE SERVICE]\n```\n";

                msg += string.Join("\n", L.Keys);

                msg += $"\n{L.Count} Language(s) are loaded\n```";
                await message.Channel.SendMessageAsync(msg);
            }

            if (message.Content == "!grt")
                await message.Channel.SendMessageAsync(L[u_loc].Greetings.intro);

            if (message.Content == "Hello" || message.Content == "こんにちは")
                await message.Channel.SendMessageAsync(L[u_loc].Greetings.hello);

            if (message.Content == "Good Morning" || message.Content == "おはよう")
                await message.Channel.SendMessageAsync(L[u_loc].Greetings.goodmorning);

            if (message.Content == "Good Night" || message.Content == "おやすみ")
                await message.Channel.SendMessageAsync(L[u_loc].Greetings.goodnight);

            if (Regex.IsMatch(message.Content, @"^\s*Hello\s?World\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(L[u_loc].Somerets.beepbeep);

            #region MusicService

            if (Regex.IsMatch(message.Content, "^!play .+$"))
            {
                await MusicService.PlayAudio(message,u_loc);
            }

            if (Regex.IsMatch(message.Content, "^!stop$"))
            {
                await MusicService.StopAudio(message,u_loc);
            }

            if (Regex.IsMatch(message.Content, "^!playing$"))
            {
                await MusicService.CheckPlaying(message, u_loc);
            }

            if (Regex.IsMatch(message.Content, "^!repeat$"))
            {
                await MusicService.SwitchRepeat(message, u_loc);
            }

            #endregion MusicService
        }


        public static string GetUserTime(ulong UserId, string loc)
        {
            DateTime utc = DateTime.UtcNow;
            if (!U.ContainsKey(UserId))
                return L[loc].ErrorMessages.notimezone;

            if (U[UserId].TimeZone == null)
                return L[loc].ErrorMessages.notimezone;

            DateTime ctime = utc + U[UserId].TimeZone;

            return L[loc].Informations.curtime.Replace("%time%", ctime.ToShortTimeString()).Replace("%zone%", U[UserId].TimeZone.ToString());
        }
    }
}