﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Remote.Broadlink;
using Remote.Neeo;
using Remote.Neeo.Devices;
using Remote.Neeo.Server;
using Remote.Utilities;

namespace Remote.HodgePodge
{
    internal static class Program
    {
        private static async Task LearnCodes(RMDevice device)
        {
            string? fileName = Program.QueryFileName();
            if (fileName == null)
            {
                return;
            }
            Dictionary<string, string> dictionary = new();
            while (true)
            {
                if (Query("Command name?") is not string name)
                {
                    break;
                }
                await device.BeginLearning();
                await device.WaitForAck();
                byte[] data = await device.WaitForData();
                dictionary[name] = data.ToHex();
            }
            File.WriteAllText(fileName, JsonSerializer.Serialize(dictionary), Encoding.UTF8);
        }

        private static async Task Main()
        {
            var brain = await BrainDiscovery.GetFirstBrainAsync();

            DeviceBuilder d = DeviceBuilder.BuildDevice("abc", DeviceType.TV);
            d.SetManufacturer(new string('1', 48));
            StartServer.SS();
        }

        private static async Task MainASRM()
        {
            PgpMethods.GenerateKeys();

            //StartServer.SS();

            using RMDeviceDiscovery discovery = new();
            using RMDevice rmDevice = await discovery.DiscoverDeviceAsync();
            while (true)
            {
                Console.Write("Mode: (0 - Learn, 1 - Test, else quit): ");
                switch (Console.ReadLine())
                {
                    case "0":
                        await Program.LearnCodes(rmDevice);
                        break;
                    case "1":
                        await Program.TestCodes(rmDevice);
                        break;
                    default:
                        return;
                }
            }
        }

        private static string? Query(string prompt, string quitCommand = "Done")
        {
            Console.Write($"{prompt} ({quitCommand} to end) ");
            return Console.ReadLine() is string text && !text.Equals(quitCommand, StringComparison.OrdinalIgnoreCase)
                ? text
                : null;
        }

        private static string? QueryFileName()
        {
            return Program.Query("What is the device name?") is string name
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"commands_{name}.json")
                : null;
        }

        private static async Task TestCodes(RMDevice device)
        {
            string? fileName = Program.QueryFileName();
            if (fileName == null)
            {
                return;
            }

            Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fileName, Encoding.UTF8))!;
            while (true)
            {
                if (Program.Query("Command name?") is not string name)
                {
                    return;
                }
                if (!dictionary.TryGetValue(name, out string? text))
                {
                    Console.Error.WriteLine($"Command {name} not found");
                    continue;
                }
                await device.SendData(ByteArray.FromHex(text));
                await device.WaitForAck();
            }
        }
    }
}
