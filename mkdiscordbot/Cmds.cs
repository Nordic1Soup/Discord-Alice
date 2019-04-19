using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mkdiscordbot
{
    internal class Cmds
    {
        public async static Task CmdHandle(SocketMessage message)
        {
            if (P.LOCK) return;

            if (message.Author.Id == P._client.CurrentUser.Id || message.Author.IsBot)
                return;

            #region detect_lang

            string u_loc = "";

            foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
            {
                foreach (ServerSetting.Role srole in P.S.Roles)
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

            if (message.Content.StartsWith("!! ! "))
            {
                if (!P.S.AdminIds.Contains(message.Author.Id))
                {
                    await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.noadminpermission);
                    return;
                }
                string[] cmd = Regex.Replace(message.Content, @"^!! ! (?<cmd>.+)$", "${cmd}").Split(' ');
                await P.Mcmd(cmd.ToList());
            }

            #region TimeService

            if (TimeService.UserHavetoSleep(message.Author.Id))
            {
                P.U[message.Author.Id].HaveToSleep = DateTime.MinValue;

                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.Sleeptime.Replace("%name%",message.Author.Username));
            }

            if (Regex.IsMatch(message.Content, "^!set timezone (\\+|-)?[0-9]{1,2}:[0-9]{2}"))
            {
                string tzone = Regex.Replace(message.Content, "^!set timezone (\\+)?(?<tzone>(-)?[0-9]{1,2}:[0-9]{2})", "${tzone}");
                TimeSpan TimeZone = TimeSpan.Parse(tzone);
                DateTime Now = DateTime.UtcNow.Add(TimeZone);
                Console.WriteLine($"{message.Author.Username} wants to change Timezone to \"{tzone}\" (Current:{Now.ToShortTimeString()})");

                if (!P.U.ContainsKey(message.Author.Id))
                    P.U.Add(message.Author.Id, new UserDictionary(message.Author.Id));
                P.U[message.Author.Id].TimeZone = TimeZone;

                await message.Channel.SendMessageAsync(P.L[u_loc].UserSettingReplys.timezoneset);
            }

            if (message.Content == "!time")
            {
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(message.Author.Id, u_loc));
            }

            if (Regex.IsMatch(message.Content, "^!time <@[0-9]+>"))
            {
                ulong uid = ulong.Parse(Regex.Replace(message.Content, "^!time <@(?<tzone>[0-9]+)>", "${tzone}"));

                await message.Channel.SendMessageAsync(TimeService.GetUserTime(uid, u_loc));
            }

            if (Regex.IsMatch(message.Content, @"^What time is it now\s?(\.|\?)?\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(message.Author.Id, u_loc));

            if (Regex.IsMatch(message.Content, @"^(今|いま)、?(何時|なんじ)(。|？|\?)\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(message.Author.Id, u_loc));

            if (Regex.IsMatch(message.Content, "^!set sleep [0-9]{1,2}:[0-9]{2}"))
            {
                string sleep = Regex.Replace(message.Content, "^!set sleep (?<time>[0-9]{1,2}:[0-9]{2})", "${time}");

                await message.Channel.SendMessageAsync(TimeService.SetUserSleep(message.Author.Id,u_loc,sleep));
            }

            #endregion TimeService

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("[ALICE CHATTING SUPPORTER] says: pong!");

            if (message.Content == "!help")
                await message.Channel.SendMessageAsync("https://mkaraki.github.io/Discord-Alice/cmds/");

            if (message.Content == "!github")
                await message.Channel.SendMessageAsync("https://github.com/mkaraki/Discord-Alice");

            if (message.Content == "!langs")
            {
                string msg = "[ALICE LANGUAGE SERVICE]\n```\n";

                msg += string.Join("\n", P.L.Keys);

                msg += $"\n{P.L.Count} Language(s) are loaded\n```";
                await message.Channel.SendMessageAsync(msg);
            }

            if (message.Content == "!grt")
                await message.Channel.SendMessageAsync(P.L[u_loc].Greetings.intro);

            if (message.Content == "Hello" || message.Content == "こんにちは")
                await message.Channel.SendMessageAsync(P.L[u_loc].Greetings.hello);

            if (message.Content == "Good Morning" || message.Content == "おはよう")
                await message.Channel.SendMessageAsync(P.L[u_loc].Greetings.goodmorning);

            if (message.Content == "Good Night" || message.Content == "おやすみ")
                await message.Channel.SendMessageAsync(P.L[u_loc].Greetings.goodnight);

            if (Regex.IsMatch(message.Content, @"^\s*Hello\s?World\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(P.L[u_loc].Somerets.beepbeep);

            #region MusicService

            if (Regex.IsMatch(message.Content, "^!play .+$"))
            {
                await MusicService.PlayAudio(message, u_loc);
            }

            if (Regex.IsMatch(message.Content, "^!stop$"))
            {
                await MusicService.StopAudio(message, u_loc);
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
    }
}