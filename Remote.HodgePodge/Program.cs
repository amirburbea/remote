﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Remote.Broadlink;
using Remote.Neeo;
using Remote.Neeo.Devices;
using Remote.Neeo.Devices.Discovery;
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
            var brain = await Brain.DiscoverAsync();
            if (brain == null)
            {
                Console.Error.WriteLine("Brain not found.");
                return;
            }
             Credentials c = new("user", "pass");
            string text = JsonSerializer.Serialize(c, JsonSerialization.Options);

            var doc = JsonDocument.Parse(text);







            try
            {
                var builder = Device.Create("test", DeviceType.Accessory)
                    .SetManufacturer("Manufacturer")
                    .AddButtons(KnownButtons.PowerOn | KnownButtons.PowerOff)
                    .AddButtonGroup(ButtonGroup.NumberPad)
                    .AddButtons(KnownButtons.Menu)
                    .SetButtonHandler((deviceId, button) =>
                    {
                        Console.WriteLine($"{deviceId}|{button}");
                        return Task.CompletedTask;
                    });

                var info = await brain.GetSystemInfoAsync();
                await brain.StartServerAsync("C#", new[] { builder });
                await Task.Delay(10000);
            }
            finally
            {
                await brain.StopServerAsync();
            }
        }

        private static async Task MainASRM()
        {
            using RMDevice? remote = await RMDiscovery.DiscoverDeviceAsync();
            if (remote == null)
            {
                return;
            }
            while (true)
            {
                Console.Write("Mode: (0 - Learn, 1 - Test, else quit): ");
                switch (Console.ReadLine())
                {
                    case "0":
                        await Program.LearnCodes(remote);
                        break;
                    case "1":
                        await Program.TestCodes(remote);
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

        private static async Task TestCodes(RMDevice remote)
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
                await remote.SendData(ByteArray.FromHex(text));
                await remote.WaitForAck();
            }
        }
    }
}
