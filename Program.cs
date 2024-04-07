using Minecraft.Net;

namespace Minecraft;

static class Program
{
    static Client InitClient() => new Client
    {
        Username = "TestBot",
    };

    static async Task Main(string[] args)
    {
        using var client = InitClient();
        await client.Connect("localhost", 25565);
        await Task.Delay(1000);

        while (true)
        {
            if (!client.Connected)
                break;

            if (!Console.KeyAvailable)
                continue;

            var key = Console.ReadKey(true).Key;

            int deltaX = 0, deltaZ = 0;

            if (key == ConsoleKey.W)
                deltaZ = 1;
            else if (key == ConsoleKey.S)
                deltaZ = -1;

            if (key == ConsoleKey.A)
                deltaX = 1;
            else if (key == ConsoleKey.D)
                deltaX = -1;

            client?.UpdateMovement(deltaX, deltaZ, key switch
            {
                ConsoleKey.Spacebar => 1,
                ConsoleKey.C => -1,
                _ => 0
            });

            await Task.Delay(1);
        }
    }
}
