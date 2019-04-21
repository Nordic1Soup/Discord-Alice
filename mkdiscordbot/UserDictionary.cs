using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace mkdiscordbot
{
    internal class UserDictionary_Service
    {
        

        
    }

    internal class UserDictionary
    {
        public static Dictionary<ulong,UserDictionary> Get(string Path)
        {
            Dictionary<ulong, UserDictionary> toret = new Dictionary<ulong, UserDictionary>();

            foreach (string path in System.IO.Directory.GetFiles(Path, "*.json"))
            {
                string json = System.IO.File.ReadAllText(path);
                ulong UserId = ulong.Parse(System.IO.Path.GetFileNameWithoutExtension(path));
                Console.WriteLine($"Loading User Settings (\"{UserId}\")");
                toret.Add(UserId, JsonConvert.DeserializeObject<UserDictionary>(json));
            }

            return toret;
        }

        public static void Save(string Path, UserDictionary[] Data)
        {
            foreach (UserDictionary ud in Data)
            {
                string json = JsonConvert.SerializeObject(ud);
                System.IO.File.WriteAllText(System.IO.Path.Combine(Path, $"{ud.Id}.json"), json);
            }
        }

        public UserDictionary(ulong _Id)
        {
            Id = _Id;
        }

        public ulong Id { get; set; }

        public TimeSpan TimeZone = new TimeSpan(255,0,0);

        public ulong NsfwCount = 0;

        public ulong Level = 0;

        public ulong Balance = 0;

        public ulong CityId = 0;

        public DateTime HaveToSleep { get; set; }
    }
}