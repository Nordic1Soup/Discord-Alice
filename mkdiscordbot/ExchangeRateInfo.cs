using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace mkdiscordbot
{
    class ExchangeRateInfo
    {
        public Quote[] quotes { get; set; }

        public class Quote
        {
            public string high { get; set; }
            public string open { get; set; }
            public string bid { get; set; } // Rate
            public string currencyPairCode { get; set; }
            public string ask { get; set; }
            public string low { get; set; }
        }

    }

    class ExchangeRateCont
    {
        private static DateTime LastGet = DateTime.MinValue;

        public ExchangeRateCont()
        {
            Update();
        }

        public void Update()
        {
            if (LastGet >= DateTime.Now.AddHours(1)) return;

            LastGet = DateTime.Now;

            string url = "https://www.gaitameonline.com/rateaj/getrate";
            string json = (new System.Net.WebClient()).DownloadString(url);
            ExchangeRateInfo eri = JsonConvert.DeserializeObject<ExchangeRateInfo>(json);

            Infos.Clear();
            Support.Clear();

            foreach (ExchangeRateInfo.Quote ei in eri.quotes)
            {
                string Frm = ei.currencyPairCode.Substring(0, 3);
                string Dst = ei.currencyPairCode.Substring(3,3);
                float Rate = float.Parse(ei.bid);

                Infos.Add(new Info()
                {
                    From = Frm,
                    To = Dst,
                    Rate = Rate,
                    Div = false
                });

                Infos.Add(new Info()
                {
                    From = Dst,
                    To = Frm,
                    Rate = Rate,
                    Div = true
                });

                Support.Add($"{Frm}/{Dst}");
                Support.Add($"{Dst}/{Frm}");
            }
        }

        public List<string> Support = new List<string>();

        public List<Info> Infos = new List<Info>();

        public class Info
        {
            public float Rate { get; set; }
            public bool Div { get; set; }
            public string From { get; set; }
            public string To { get; set; }
        }

    }
}
