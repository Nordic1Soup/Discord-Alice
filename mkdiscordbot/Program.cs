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
        public static string AliceDataDir;
        public static string AliceLocaleDir;

        public static Dictionary<ulong, DiscordServerInformation> I;

        public static Dictionary<string, LocaleDef> L;

        private static string Token;
        public static DiscordSocketClient _client;

        public static bool LOCK = true;

        private static void Main(string[] args)
        {
            string CDir = System.IO.Directory.GetCurrentDirectory();
            AliceDataDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Alice_Data");

            if (!System.IO.Directory.Exists(AliceDataDir))
            {
                System.IO.Directory.CreateDirectory(AliceDataDir);
                Console.WriteLine("No Alice Data");
                Environment.Exit(1);
            }

            AliceLocaleDir = System.IO.Path.Combine(CDir, "Locale");

            if (!System.IO.Directory.Exists(AliceLocaleDir) || System.IO.Directory.GetFiles(AliceLocaleDir, "*-*.json").Length < 1)
            {
                Console.WriteLine("No Locale Files");
                Environment.Exit(1);
            }
            Console.WriteLine("Applying Server Settings");
            Locale.InitLocaleInfo();

            string tokenpath = System.IO.Path.Combine(AliceDataDir, "token");

            if (!System.IO.File.Exists(tokenpath))
            {
                Console.WriteLine("No Token");
                Environment.Exit(1);
            }
            Token = System.IO.File.ReadAllText(tokenpath);

            string[] serverInfos = System.IO.Directory.GetDirectories(AliceDataDir, "*", System.IO.SearchOption.TopDirectoryOnly);
            I = new Dictionary<ulong, DiscordServerInformation>();
            foreach (string svid in serverInfos)
            {
                try
                {
                    ulong sv = ulong.Parse(System.IO.Path.GetFileName(svid));
                    I.Add(sv, new DiscordServerInformation(svid));
                }
                catch (Exception)
                { }
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

            ulong sv = arg.Guild.Id;

            if (!I.ContainsKey(sv)) return;

            if (I[sv].S.WelcomeMessage)
            {
                foreach (string lang in I[sv].S.InformationLanguage)
                {
                    str += $"--- {lang} --------------------\n";
                    str += L[lang].Informations.welcomemsg.Replace("%name%", arg.Username) + "\n";
                }

                await ((ISocketMessageChannel)_client.GetChannel(I[sv].S.GeneralId)).SendMessageAsync(str);
            }
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            foreach (DiscordServerInformation dsi in I.Values)
            {
                I[dsi.Id].Guild = _client.GetGuild(dsi.Id);
            }

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
                    foreach (DiscordServerInformation dsi in I.Values)
                    {
                        dsi.Save();

                        if (dsi.S.ExitMessage)
                        {
                            foreach (ServerSetting.Informationchannel ich in dsi.S.InformationChannels)
                            {
                                await ((ISocketMessageChannel)_client.GetChannel(ich.id))
                                    .SendMessageAsync(L[ich.lang].Informations.shutdown);
                            }
                        }
                    }

                    Environment.Exit(0);
                    break;

                case "save":
                    LOCK = true;
                    foreach (DiscordServerInformation dsi in I.Values)
                        dsi.Save();
                    Console.WriteLine("Userdata saved");
                    LOCK = false;
                    break;

                case "ram":
                    Console.WriteLine($"RAM: {Environment.WorkingSet.ToString("N0")}");
                    break;

                case "cleanram":
                    LOCK = true;
                    foreach (DiscordServerInformation dsi in I.Values)
                        dsi.Save();
                    Console.WriteLine("GC Collect");
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
                    foreach (DiscordServerInformation dsi in I.Values)
                        await ((ISocketMessageChannel)_client.GetChannel(dsi.S.GeneralId)).SendMessageAsync(broadcast_str);
                    break;

                case "say":
                    List<string> say_str = cmds.ToList();
                    say_str.RemoveAt(0);
                    ulong chid = ulong.Parse(say_str[0]);
                    say_str.RemoveAt(0);
                    string say_mstr = string.Join(" ", say_str);
                    await ((ISocketMessageChannel)_client.GetChannel(chid)).SendMessageAsync(say_mstr);
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

            foreach (DiscordServerInformation dsi in I.Values)
            {
                dsi.Save();

                if (dsi.S.StartMessage)
                {
                    foreach (ServerSetting.Informationchannel ich in dsi.S.InformationChannels)
                    {
                        await ((ISocketMessageChannel)_client.GetChannel(ich.id))
                            .SendMessageAsync(L[ich.lang].Informations.booted);
                    }
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