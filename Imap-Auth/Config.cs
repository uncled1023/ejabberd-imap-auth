using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImapAuth
{
    public class Config
    {
        public int MaxBuffer { get; set; }

        // IMAP Settings
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public bool ValidateCert { get; set; }

        public Config()
        {
            MaxBuffer = 1000;
            Host = "localhost";
            Port = 993;
            UseSSL = true;
            ValidateCert = false;
        }
    }
}
