﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace mkdiscordbot
{
    internal class Locale
    {
        public static void InitLocaleInfo()
        {
            Program.L = new Dictionary<string, LocaleDef>();

            foreach (string path in System.IO.Directory.GetFiles("Locale", "*-*.json"))
            {
                string json = System.IO.File.ReadAllText(path);
                string lname = System.IO.Path.GetFileNameWithoutExtension(path);
                System.Console.WriteLine($"Loading Locale (\"{lname}\")");
                Program.L.Add(lname,JsonConvert.DeserializeObject<LocaleDef>(json));
            }
        }
    }

    internal class LocaleDef
    {
        public Information Informations { get; set; }
        public Greeting Greetings { get; set; }
        public UserSettingReply UserSettingReplys { get; set; }
        public Someret Somerets { get; set; }
        public ErrorMessage ErrorMessages { get; set; }

        public class ErrorMessage
        {
            public string notimezone { get; set; }
            public string nomusicch { get; set; }
            public string unknowncmd { get; set; }
        }

        public class Someret
        {
            public string beepbeep { get; set; }
        }

        public class UserSettingReply
        {
            public string timezoneset { get; set; }
        }

        public class Information
        {
            public string shutdown { get; set; }
            public string booted { get; set; }
            public string welcomemsg { get; set; }
            public string curtime { get; set; }
            public string playto { get; set; }
            public string playing { get; set; }
            public string notplaying { get; set; }
        }

        public class Greeting
        {
            public string intro { get; set; }
            public string hello { get; set; }
            public string goodmorning { get; set; }
            public string goodnight { get; set; }
        }
    }
}