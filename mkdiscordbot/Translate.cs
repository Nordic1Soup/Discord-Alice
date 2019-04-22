using System;
using System.Collections.Generic;
using System.Text;

namespace mkdiscordbot
{
    class Translate
    {
        public static string UrlEncode(string Text)
            => System.Web.HttpUtility.UrlEncode(Text);

        public static string TranslateUrl(ulong sid,string Orig, string Dst, bool Simple = false)
        {
            string burl = "";
            if (Simple)
            {
                burl = P.I[sid].S.GASTranslater;
                burl += $"?text={UrlEncode(Orig)}&dest={UrlEncode(Dst)}";
            }
            

            return burl;
        }
    }
}
