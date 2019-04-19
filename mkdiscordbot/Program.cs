using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mkdiscordbot
{
    internal class P
    {
        public static ServerSetting S;
        public static Dictionary<string, LocaleDef> L;
        public static Dictionary<ulong, UserDictionary> U;

        private static string Token;
        public static DiscordSocketClient _client;
        public static SocketGuild Guild;

        public static bool LOCK = true;

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
            new P().MainAsync().GetAwaiter().GetResult();
        }

        public P()
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

            if (S.WelcomeMessage)
            {
                foreach (string lang in S.InformationLanguage)
                {
                    str += $"--- {lang} --------------------\n";
                    str += L[lang].Informations.welcomemsg.Replace("%name%", arg.Username) + "\n";
                }

                await ((ISocketMessageChannel)_client.GetChannel(S.GeneralId)).SendMessageAsync(str);
            }
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            Guild = _client.GetGuild(S.ServerID);

            Console.WriteLine("Type 'exit' to exit");
            while (true)
            {
                string[] cmds = Console.ReadLine().Split(' ');

                await Mcmd(cmds.ToList());
            }
        }

        public static async Task Mcmd(List<string> cmds)
        {
            switch (cmds[0])
            {
                case "exit":
                    LOCK = true;
                    UserDictionary_Service.Save();
                    Console.WriteLine("Userdata saved");
                    if (S.ExitMessage)
                    {
                        foreach (ServerSetting.Informationchannel ich in S.InformationChannels)
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

                case "cleanram":
                    LOCK = true;
                    UserDictionary_Service.Save();
                    GC.Collect();
                    LOCK = false;
                    break;

                case "clear":
                    Console.Clear();
                    break;

                case "broadcast":
                    List<string> str = cmds.ToList();
                    str.RemoveAt(0);
                    string mstr = string.Join(" ", str);
                    string broadcast_str =
                        "\n----- There is a broad cast message from bot administrator." +
                        "\n----- The message language is not translated by Alice Developper Team." +
                        "\n----- This message is not posted from Alice internal management system." +
                        "\n" +
                        "```\n" +
                        mstr +
                        "\n```";
                    await ((ISocketMessageChannel)_client.GetChannel(S.GeneralId)).SendMessageAsync(broadcast_str);
                    break;
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

            if (S.StartMessage)
            {
                foreach (ServerSetting.Informationchannel ich in S.InformationChannels)
                {
                    await ((ISocketMessageChannel)_client.GetChannel(ich.id))
                        .SendMessageAsync(L[ich.lang].Informations.booted);
                }
            }

            await _client.SetGameAsync("https://github.com/mkaraki/Discord-Alice");

            LOCK = false;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            _ = Task.Run(() => Cmds.CmdHandle(message));
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    }
}