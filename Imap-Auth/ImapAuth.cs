using MailKit;
using MailKit.Net.Imap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImapAuth
{
    public class ImapAuth
    {
        public static bool StopProcess = false;

        public static void Main(string[] args)
        {
            try
            {
                // Load the configuration.  If none exists, create a default one
                string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json");
                if (!File.Exists(configPath))
                {
                    string defaultConfig = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
                    File.WriteAllText(configPath, defaultConfig);
                }
                string configStr = File.ReadAllText(configPath);
                Config config = JsonConvert.DeserializeObject<Config>(configStr);

                // Get the input stream
                var outputMessage = new char[config.MaxBuffer]; // buffer
                var stdinStream = Console.OpenStandardInput();
                using (var binaryReader = new BinaryReader(stdinStream))
                {
                    // Process the stream until we decide to stop
                    while (!StopProcess)
                    {
                        bool success = true;

                        // Read data from stdin
                        var b1 = binaryReader.ReadByte();
                        var b2 = binaryReader.ReadByte();

                        // Perform big endian conversion
                        var expectedInputLength = b2 + (b1 * 256);

                        // Check for buffer overrun
                        if (expectedInputLength > config.MaxBuffer)
                        {
                            throw new InternalBufferOverflowException(string.Format("{0} > {1}", expectedInputLength,
                            config.MaxBuffer));
                        }

                        // Read opcode, username and password
                        var bytes = binaryReader.ReadBytes(expectedInputLength);
                        var data = Encoding.ASCII.GetString(bytes);
                        string[] elements = data.Split(':');

                        success = ProcessCommand(config, elements);

                        // Prepare return value, first short is always 2
                        outputMessage[0] = (char)0;
                        outputMessage[1] = (char)2;

                        // Second short is 1 for success, 0 for failure
                        outputMessage[2] = (char)0;
                        outputMessage[3] = (char)(success ? 1 : 0); // or 0 for failure

                        // Send return value
                        Console.Out.Write(outputMessage, 0, 4);
                    }
                }
            }
            catch (Exception)
            {
                // Something happened, lets just quitely leave...
            }
        }

        private static bool ProcessCommand(Config config, string[] data)
        {
            bool success = false;

            if (data.Length > 0)
            {
                // log here the command
            }

            // Only process the command if we have 4 or more elements
            if (data.Length >= 4)
            {
                // Get the command type
                string command = data[0];

                // Change to your desired username pattern.  ie: username@domain
                string username = string.Format("{0}@{1}", data[1], data[2]);
                string password = data[3];

                // process command
                switch (command)
                {
                    case "isuser":
                        success = CheckPass(config, username, password);
                        break;
                    case "auth":
                        success = CheckPass(config, username, password);
                        break;
                    case "setpass":
                        // don't let them change their password
                        success = false;
                        break;
                    default:
                        // We don't know it, so we should quit
                        StopProcess = true;
                        break;
                }
            }

            return success;
        }

        private static bool CheckPass(Config config, string username, string password)
        {
            bool valid = true;
            using (var client = new ImapClient())
            {
                // For demo-purposes, accept all SSL certificates
                if (!config.ValidateCert)
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                }

                try
                {
                    client.Connect(config.Host, config.Port, true);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(username, password);
                }
                catch (Exception)
                {
                    // Exception thrown, they didn't auth correctly, or we couldn't connect
                    valid = false;
                }

                try
                {
                    client.Disconnect(true);
                }
                catch (Exception) // safe to catch and not worry
                { }
            }
            return valid;
        }
    }
}
