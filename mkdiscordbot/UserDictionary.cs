using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace mkdiscordbot
{
    internal class UserDictionary_Service
    {
        public static void Load()
        {
            Program.U = new Dictionary<ulong, UserDictionary>();

            foreach (string path in System.IO.Directory.GetFiles("Users", "*.json"))
            {
                string json = System.IO.File.ReadAllText(path);
                ulong UserId = ulong.Parse(System.IO.Path.GetFileNameWithoutExtension(path));
                Console.WriteLine($"Loading User Settings (\"{UserId}\")");
                Program.U.Add(UserId, JsonConvert.DeserializeObject<UserDictionary>(json));
            }
        }

        public static void Save()
        {
            foreach (UserDictionary ud in Program.U.Values)
            {
                string json = JsonConvert.SerializeObject(ud);
                System.IO.File.WriteAllText(System.IO.Path.Combine("Users",$"{ud.Id}.json"),json);
            }
        }
    }

    internal class UserDictionary
    {
        public UserDictionary(ulong _Id)
        {
            Id = _Id;
        }

        public ulong Id { get; set; }
        public TimeSpan TimeZone { get; set; }
        public ulong NsfwCount { get; set; }
    }
}