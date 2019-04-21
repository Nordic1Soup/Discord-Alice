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

            ulong sid = ((SocketGuildChannel)message.Channel).Guild.Id;

            if (!P.I.Keys.Contains(sid))
            {
                await message.Channel.SendMessageAsync("NOT PERMITTED REQUEST");
                return;
            }

            #region detect_lang

            string u_loc = "";

            foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
            {
                foreach (ServerSetting.Role srole in P.I[sid].S.Roles)
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

            //if (Regex.IsMatch(message.Content, @"^!set lang ((a-zA-Z)|\-)+"))
            //{
            //    string loc_zone = Regex.Replace(message.Content, @"^!set timezone (?<loc>((a-zA-Z)|\-)+)", "${loc}");

            //    await message.Channel.SendMessageAsync("Language set");
            //}

            if (message.Content.StartsWith("!! ! "))
            {
                if (!P.I[sid].S.AdminIds.Contains(message.Author.Id))
                {
                    await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.noadminpermission);
                    return;
                }
                string[] cmd = Regex.Replace(message.Content, @"^!! ! (?<cmd>.+)$", "${cmd}").Split(' ');
                await P.Mcmd(cmd.ToList());
            }

            #region TimeService

            if (TimeService.UserHavetoSleep(sid, message.Author.Id))
            {
                P.I[sid].U[message.Author.Id].HaveToSleep = DateTime.MinValue;

                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.Sleeptime.Replace("%name%",message.Author.Username));
            }

            if (Regex.IsMatch(message.Content, "^!set timezone (\\+|-)?[0-9]{1,2}:[0-9]{2}"))
            {
                string tzone = Regex.Replace(message.Content, "^!set timezone (\\+)?(?<tzone>(-)?[0-9]{1,2}:[0-9]{2})", "${tzone}");
                TimeSpan TimeZone = TimeSpan.Parse(tzone);
                DateTime Now = DateTime.UtcNow.Add(TimeZone);
                Console.WriteLine($"{message.Author.Username} wants to change Timezone to \"{tzone}\" (Current:{Now.ToShortTimeString()})");

                if (!P.I[sid].U.ContainsKey(message.Author.Id))
                    P.I[sid].U.Add(message.Author.Id, new UserDictionary(message.Author.Id));
                P.I[sid].U[message.Author.Id].TimeZone = TimeZone;

                await message.Channel.SendMessageAsync(P.L[u_loc].UserSettingReplys.timezoneset);
            }

            if (message.Content == "!time")
            {
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(sid, message.Author.Id, u_loc));
            }

            if (Regex.IsMatch(message.Content, "^!time <@[0-9]+>"))
            {
                ulong uid = ulong.Parse(Regex.Replace(message.Content, "^!time <@(?<tzone>[0-9]+)>", "${tzone}"));

                await message.Channel.SendMessageAsync(TimeService.GetUserTime(sid, uid, u_loc));
            }

            if (Regex.IsMatch(message.Content, @"^What time is it now\s?(\.|\?)?\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(sid, message.Author.Id, u_loc));

            if (Regex.IsMatch(message.Content, @"^(今|いま)、?(何時|なんじ)(。|？|\?)\s*$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync(TimeService.GetUserTime(sid, message.Author.Id, u_loc));

            if (Regex.IsMatch(message.Content, "^!set sleep [0-9]{1,2}:[0-9]{2}"))
            {
                string sleep = Regex.Replace(message.Content, "^!set sleep (?<time>[0-9]{1,2}:[0-9]{2})", "${time}");

                await message.Channel.SendMessageAsync(TimeService.SetUserSleep(sid, message.Author.Id,u_loc,sleep));
            }

            #endregion TimeService

            #region MoneyService

            if (message.Content == "!bal")
                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.money.Replace("%mon%",MoneyService.GetBalance(sid,message.Author.Id).ToString()));

            if (message.Content == "!work")
            {
                ulong bal = MoneyService.GetBalance(sid,message.Author.Id);
                ulong mon = MoneyService.GetMoney();

                MoneyService.SetBalance(sid,message.Author.Id, bal+mon);
                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.money.Replace("%mon%", mon.ToString()));
            }

            #endregion MoneyService

            #region ExchangeService

            if (Regex.IsMatch(message.Content, @"^!exc [A-Z]{3}/[A-Z]{3} (\d|\.)+$"))
            {
                string[] isv = Regex.Replace(message.Content, @"^!exc (?<frm>[A-Z]{3})/(?<dst>[A-Z]{3}) (?<org>(\d|\.)+)$", "${frm}/${dst}/${org}").Split('/');

                float mon = ExchangeService.Calc(isv[0], isv[1], float.Parse(isv[2]));

                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.excinfo.Replace("%to%", ExchangeService.V_Format(isv[1], mon)));
            }

            if (message.Content == "!exc update")
            {
                ExchangeService.erc.Update();
            }

            #endregion ExchangeService

            #region WeatherService

            if (message.Content == "!weather")
            {
                if (WeatherService.UserHasLocInfo(sid,message.Author.Id))
                {
                    string wstr = WeatherService.WstrUser(sid, message.Author.Id);
                    await message.Channel.SendMessageAsync(P.L[u_loc].Informations.cweather.Replace("%w%", wstr) + "\n" + WeatherService.WeatherInfoSource());
                }
                else
                    await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.nolocationinfo);
            }

            if (Regex.IsMatch(message.Content, @"^!set location [0-9]+$"))
            {
                ulong lid = ulong.Parse(Regex.Replace(message.Content, @"^!set location (?<id>[0-9]+)$", "${id}"));

                WeatherService.SetUserLoc(sid, message.Author.Id,lid);

                await message.Channel.SendMessageAsync(P.L[u_loc].UserSettingReplys.locinfoset);
            }

            if (Regex.IsMatch(message.Content, @"^!weather [0-9]+$"))
            {
                ulong cid = ulong.Parse( Regex.Replace(message.Content, @"^!weather (?<id>[0-9]+)$", "${id}"));

                string wstr = WeatherService.Wstr(sid, cid);

                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.cweather.Replace("%w%", wstr) + "\n" + WeatherService.WeatherInfoSource());
            }

            if (Regex.IsMatch(message.Content, "^!weather <@[0-9]+>"))
            {
                ulong uid = ulong.Parse(Regex.Replace(message.Content, "^!weather <@(?<tzone>[0-9]+)>", "${tzone}"));

                if (WeatherService.UserHasLocInfo(sid, uid))
                {
                    string wstr = WeatherService.WstrUser(sid, uid);
                    await message.Channel.SendMessageAsync(P.L[u_loc].Informations.cweather.Replace("%w%", wstr) + "\n" + WeatherService.WeatherInfoSource());
                }
                else
                    await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.nolocationinfo);
            }


            #endregion WeatherService

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

            if (Regex.IsMatch(message.Content, @"^See you later alligator$", RegexOptions.IgnoreCase))
                await message.Channel.SendMessageAsync("In a while crocodile.");

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