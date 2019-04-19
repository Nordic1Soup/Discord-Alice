using System;
using System.Collections.Generic;
using System.Text;

namespace mkdiscordbot
{
    class TimeService
    {
        public static string GetUserTime(ulong UserId, string loc)
        {
            DateTime utc = DateTime.UtcNow;
            TimeSpan? tspan = UserTimeZone(UserId);

            if (tspan == null)
                return P.L[loc].ErrorMessages.notimezone;

            DateTime ctime = utc + (tspan ?? TimeSpan.Zero);

            return P.L[loc].Informations.curtime.Replace("%time%", ctime.ToShortTimeString()).Replace("%zone%", (tspan ?? TimeSpan.Zero).ToString());
        }

        public static TimeSpan? UserTimeZone(ulong id)
        {
            if (!P.U.ContainsKey(id))
                return null;
            else if (P.U[id].TimeZone.Hours == 255)
                return null;

            return P.U[id].TimeZone;
        }

        public static bool UserhasTimeZone(ulong id)
        {
            if (UserTimeZone(id) == null) return false;
            else return true;
        }

        public static string SetUserSleep(ulong UserId, string loc,string stime)
        {
            if (!UserhasTimeZone(UserId))
                return P.L[loc].ErrorMessages.notimezone;

            DateTime stv = DateTime.Parse(stime);
            if (stv <= DateTime.UtcNow.Add(UserTimeZone(UserId) ?? TimeSpan.Zero))
                stv.AddDays(1);

            P.U[UserId].HaveToSleep = stv;

            return P.L[loc].UserSettingReplys.sleeptimeset;
        }

        public static bool UserhasSleepTime(ulong id)
        {
            if (!P.U.ContainsKey(id))
                return false;
            else if (!UserhasTimeZone(id))
                return false;
            else if (P.U[id].HaveToSleep <= DateTime.UtcNow.Add(UserTimeZone(id) ?? TimeSpan.Zero).AddHours(-3))
                return false;
            else
                return true;
        }

        public static bool UserHavetoSleep(ulong id)
        {
            if (!UserhasSleepTime(id)) return false;
            if (P.U[id].HaveToSleep >= DateTime.UtcNow.Add(UserTimeZone(id) ?? TimeSpan.Zero)) return true;
            else return false;
        }
    }
}
