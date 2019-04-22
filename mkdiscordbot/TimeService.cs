using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace mkdiscordbot
{
    class TimeService
    {
        public static Embed GetUserTime(ulong sid,ulong UserId, string loc)
        {
            DateTime utc = DateTime.UtcNow;
            TimeSpan? tspan = UserTimeZone(sid,UserId);

            if (tspan == null)
                //return P.L[loc].ErrorMessages.notimezone;
                return new EmbedBuilder() { Description = P.L[loc].ErrorMessages.notimezone }.Build();

            DateTime ctime = utc + (tspan ?? TimeSpan.Zero);

            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.Orange;
            eb.Title = "Time (" + P._client.GetUser(UserId).Username + ")";
            eb.Description = ctime.ToString("yyyy/MM/dd HH:mm");
            eb.Footer = new EmbedFooterBuilder();
            eb.Footer.Text = "UTC"+(tspan ?? TimeSpan.Zero).ToString();

            return eb.Build();
            //return P.L[loc].Informations.curtime.Replace("%time%", ctime.ToString("yyyy/MM/dd HH:mm")).Replace("%zone%", (tspan ?? TimeSpan.Zero).ToString());
        }

        public static TimeSpan? UserTimeZone(ulong sid,ulong id)
        {
            if (!P.I[sid].U.ContainsKey(id))
                return null;
            else if (P.I[sid].U[id].TimeZone.Hours == 255)
                return null;

            return P.I[sid].U[id].TimeZone;
        }

        public static bool UserhasTimeZone(ulong sid,ulong id)
        {
            if (UserTimeZone(sid,id) == null) return false;
            else return true;
        }

        public static string SetUserSleep(ulong sid,ulong UserId, string loc,string stime)
        {
            if (!UserhasTimeZone(sid,UserId))
                return P.L[loc].ErrorMessages.notimezone;

            DateTime stv = DateTime.Parse(stime);
            if (stv <= DateTime.UtcNow.Add(UserTimeZone(sid,UserId) ?? TimeSpan.Zero))
                stv.AddDays(1);

            P.I[sid].U[UserId].HaveToSleep = stv;

            return P.L[loc].UserSettingReplys.sleeptimeset;
        }

        public static bool UserhasSleepTime(ulong sid,ulong id)
        {
            if (!P.I[sid].U.ContainsKey(id))
                return false;
            else if (!UserhasTimeZone(sid,id))
                return false;
            else if (P.I[sid].U[id].HaveToSleep <= DateTime.UtcNow.Add(UserTimeZone(sid,id) ?? TimeSpan.Zero).AddHours(-3))
                return false;
            else
                return true;
        }

        public static bool UserHavetoSleep(ulong sid,ulong id)
        {
            if (!UserhasSleepTime(sid,id)) return false;
            if (P.I[sid].U[id].HaveToSleep >= DateTime.UtcNow.Add(UserTimeZone(sid,id) ?? TimeSpan.Zero)) return true;
            else return false;
        }
    }
}
