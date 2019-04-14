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
        public static Dictionary<ulong, IAudioClient> vcsessions = new Dictionary<ulong, IAudioClient>();
        public static Dictionary<ulong, PlayingInfo> PI = new Dictionary<ulong, PlayingInfo>();

        public static async Task<ulong> CheckIsMusicCH(SocketMessage message,string u_loc,bool sendmsg)
        {
            bool is_mch = false;
            ulong vcid = 0;
            foreach (ServerSetting.MusicChannel mc in Program.svSettings.MusicChannels)
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
                await message.Channel.SendMessageAsync(Program.L[u_loc].ErrorMessages.nomusicch);
            }

            return vcid;
        }

        public static async Task StopAudio(SocketMessage message, string u_loc)
        {
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0)
            {
                await message.Channel.SendMessageAsync(Program.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (vcsessions.ContainsKey(vcid))
            {
                await vcsessions[vcid].StopAsync();
                vcsessions[vcid].Dispose();
            }
        }

        public static async Task CheckPlaying(SocketMessage message, string u_loc)
        {
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0) {
                await message.Channel.SendMessageAsync(Program.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (PI.ContainsKey(vcid))
                await message.Channel.SendMessageAsync(Program.L[u_loc].Informations.playing.Replace("%content%", PI[vcid].Name));
            else
                await message.Channel.SendMessageAsync(Program.L[u_loc].Informations.notplaying);
        }

        public static async Task SwitchRepeat(SocketMessage message, string u_loc)
        {
            ulong vcid = await CheckIsMusicCH(message, u_loc, false);
            if (vcid == 0) {
                await message.Channel.SendMessageAsync(Program.L[u_loc].ErrorMessages.unknowncmd);
                return;
            }

            if (PI.ContainsKey(vcid))
            {
                if (PI[vcid].Repeat)
                {
                    PI[vcid].Repeat = false;
                    await message.Channel.SendMessageAsync("No Resource:" + "turnoffrepeat");
                }
                else {
                    PI[vcid].Repeat = true;
                    await message.Channel.SendMessageAsync("No Resource:" + "turnonrepeat");
                }
            }
            else
                await message.Channel.SendMessageAsync(Program.L[u_loc].ErrorMessages.unknowncmd);
        }

        public static  async Task PlayAudio(SocketMessage message, string u_loc)
        {
            string src = Regex.Replace(message.Content, "^!play (?<vurl>.+)$", "${vurl}");

            ulong vcid = await CheckIsMusicCH(message, u_loc, true); if (vcid == 0) return;

            if (Program.Guild == null)
            {
                var cnl = message.Channel as SocketGuildChannel;
                Program.Guild = cnl.Guild;
            }

            Console.WriteLine($"Play to \"{src}\"");

            if (!vcsessions.ContainsKey(vcid))
                vcsessions.Add(vcid, await JoinVCChannel(vcid));
            else
            {
                await vcsessions[vcid].StopAsync();
                vcsessions[vcid].Dispose();
                vcsessions[vcid] = await JoinVCChannel(vcid);
            }

            PlayingInfo pi = new PlayingInfo() {
                Url = src,
                Name = "No Informaton Music",
                Repeat = true
            };

            if (PI.ContainsKey(vcid))
                PI[vcid] = new PlayingInfo();
            else
                PI.Add(vcid, new PlayingInfo());

            await message.Channel.SendMessageAsync(Program.L[u_loc].Informations.playto.Replace("%content%",pi.Name));

            //await vcsessions[vcid].SetSpeakingAsync(true);
            await SendAsync(vcsessions[vcid], src,vcid);
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

        public static  async Task SendAsync(IAudioClient client, string path,ulong id)
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
            while (PI[id].Repeat);
        }

        [Command("joinvc", RunMode = RunMode.Async)]
        public static async Task<IAudioClient> JoinVCChannel(ulong id)
        {
            SocketVoiceChannel channel = Program.Guild.GetVoiceChannel(566490551713529870);

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
