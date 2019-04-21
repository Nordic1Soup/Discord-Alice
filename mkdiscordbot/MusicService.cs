using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mkdiscordbot
{
    class MusicService
    {
        public class ServerInfo
        {
            public Dictionary<ulong, IAudioClient> vcsessions = new Dictionary<ulong, IAudioClient>();
            public Dictionary<ulong, PlayingInfo> PI = new Dictionary<ulong, PlayingInfo>();
        }

        public static Dictionary<ulong, ServerInfo> S = new Dictionary<ulong, ServerInfo>();

        private static ulong Sid(SocketMessage message)
            => ((SocketGuildChannel)message.Channel).Guild.Id;

        public static async Task<ulong> CheckIsMusicCH(SocketMessage message,string u_loc,bool sendmsg)
        {
            bool is_mch = false;
            ulong vcid = 0;
            foreach (ServerSetting.MusicChannel mc in P.I[Sid(message)].S.MusicChannels)
            {
                if (message.Channel.Id == mc.textid)
                {
                    is_mch = true;
                    vcid = mc.vcid;
                    break;
                }
            }

            if (sendmsg && !is_mch)
            {
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.nomusicch);
            }

            return vcid;
        }

        public static async Task StopAudio(SocketMessage message, string u_loc)
        {
            ulong sid = Sid(message);
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0)
            {
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (!S.ContainsKey(sid)) return;

            if (S[sid].vcsessions.ContainsKey(vcid))
            {
                await S[sid].vcsessions[vcid].StopAsync();
                S[sid].vcsessions[vcid].Dispose();
            }
        }

        public static async Task CheckPlaying(SocketMessage message, string u_loc)
        {
            ulong sid = Sid(message);
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0) {
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (!S.ContainsKey(sid))
            {
                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.notplaying);
                return;
            }


            if (S[sid].PI.ContainsKey(vcid))
                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.playing.Replace("%content%", S[sid].PI[vcid].Name));
            else
                await message.Channel.SendMessageAsync(P.L[u_loc].Informations.notplaying);
        }

        public static async Task SwitchRepeat(SocketMessage message, string u_loc)
        {
            ulong sid = Sid(message);
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0) {
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (!S.ContainsKey(sid))
            {
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (S[sid].PI.ContainsKey(vcid))
            {
                if (S[sid].PI[vcid].Repeat)
                {
                    S[sid].PI[vcid].Repeat = false;
                    await message.Channel.SendMessageAsync(P.L[u_loc].Informations.turnoffrepeat);
                }
                else {
                    S[sid].PI[vcid].Repeat = true;
                    await message.Channel.SendMessageAsync(P.L[u_loc].Informations.turnonrepeat);
                }
            }
            else
                await message.Channel.SendMessageAsync(P.L[u_loc].ErrorMessages.unknowncmd);
        }

        public static  async Task PlayAudio(SocketMessage message, string u_loc)
        {
            ulong sid = Sid(message);
            string src = Regex.Replace(message.Content, "^!play (?<vurl>.+)$", "${vurl}");

            ulong vcid = await CheckIsMusicCH(message, u_loc, true); if (vcid == 0) return;

            if (P.I[sid].Guild == null)
            {
                var cnl = message.Channel as SocketGuildChannel;
                P.I[sid].Guild = cnl.Guild;
            }

            Console.WriteLine($"Play to \"{src}\"");

            if (!S.ContainsKey(sid))
            {
                S.Add(sid,new ServerInfo());
                return;
            }

            if (!S[sid].vcsessions.ContainsKey(vcid))
                S[sid].vcsessions.Add(vcid, await JoinVCChannel(Sid(message),vcid));
            else
            {
                await S[sid].vcsessions[vcid].StopAsync();
                S[sid].vcsessions[vcid].Dispose();
                S[sid].vcsessions[vcid] = await JoinVCChannel(Sid(message),vcid);
            }

            PlayingInfo pi = new PlayingInfo() {
                Url = src,
                Name = "No Informaton Music",
                Repeat = true
            };

            if (S[sid].PI.ContainsKey(vcid))
                S[sid].PI[vcid] = new PlayingInfo();
            else
                S[sid].PI.Add(vcid, new PlayingInfo());

            await message.Channel.SendMessageAsync(P.L[u_loc].Informations.playto.Replace("%content%",pi.Name));

            //await vcsessions[vcid].SetSpeakingAsync(true);
            await SendAsync(S[sid].vcsessions[vcid], src,vcid,sid);
        }

        public static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"" + path + "\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        public static  async Task SendAsync(IAudioClient client, string path,ulong id,ulong sid)
        {
            do
            {
                using (var ffmpeg = CreateStream(path))
                using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
                {
                    try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(discord); }
                    finally { await discord.FlushAsync(); }
                }
            }
            while (S[sid].PI[id].Repeat);
        }

        [Command("joinvc", RunMode = RunMode.Async)]
        public static async Task<IAudioClient> JoinVCChannel(ulong sid,ulong id)
        {
            SocketVoiceChannel channel = P.I[sid].Guild.GetVoiceChannel(id);

            IAudioClient con = await channel.ConnectAsync();

            return con;
            //vcsessions.Add(id, con);
        }

    }

    class PlayingInfo
    {
        public string Url { get; set; }
        public bool Repeat { get; set; }
        public string Name { get; set; }
    }
}
