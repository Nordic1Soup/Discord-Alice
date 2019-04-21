namespace mkdiscordbot
{
    internal class ExchangeService
    {
        public static ExchangeRateCont erc = new ExchangeRateCont();

        public static void Update()
        {
            erc.Update();
        }

        public static float Calc(string frm, string to, float orig)
        {
            if (!erc.Support.Contains($"{frm}/{to}"))
                return 0;

            foreach (ExchangeRateCont.Info eri in erc.Infos)
            {
                if (frm != eri.From || to != eri.To) continue;

                if (eri.Div)
                    return orig / eri.Rate;
                else
                    return orig * eri.Rate;
            }

            return 0;
        }

        public static string V_Format(string id, float mon) => $"{mon}{id}";
    }
}