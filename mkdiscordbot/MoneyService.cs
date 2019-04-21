namespace mkdiscordbot
{
    internal class MoneyService
    {
        public static ulong GetBalance(ulong sid,ulong id)
        {
            if (!P.I[sid].U.ContainsKey(id))
                return 0;
            else
                return P.I[sid].U[id].Balance;
        }

        public static void SetBalance(ulong sid, ulong id, ulong Bal)
        {
            if (!P.I[sid].U.ContainsKey(id))
                P.I[sid].U.Add(id, new UserDictionary(id));

            P.I[sid].U[id].Balance = Bal;
        }

        public static ulong GetMoney()
        {
            System.Random r = new System.Random();

            return (ulong)r.Next(100, 1000);
        }
    }
}