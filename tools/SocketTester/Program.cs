// See https://aka.ms/new-console-template for more information

using SocketTester;
using System.CommandLine;

var returnCode = 0;
var rootCommand = new RootCommand("RhoAias Socket Tester.");

var typeOption = new Option<string>(
    name: "--type", 
    description: "Socket type, value is tcp,udp etc.", 
    getDefaultValue: () => "tcp");
typeOption.AddAlias("-t");

var serverPortOption = new Option<int>(
    name:"--server-port", 
    description: "Server listen port.",
    getDefaultValue: () => 9999);
serverPortOption.AddAlias("-sp");

var serverIpOption = new Option<string>(
    name: "--server-ip",
    description: "Server ip address.",
    getDefaultValue: () => "127.0.0.1");
serverIpOption.AddAlias("-si");

var clientPortOption = new Option<int>(
    name: "--client-port",
    description: "Client connection port.",
    getDefaultValue: () => 8888);
clientPortOption.AddAlias("-cp");

var compressedOption = new Option<bool>(
    name: "--compressed", 
    description: "Enable stream compression.",
    getDefaultValue: () => false);
compressedOption.AddAlias("-c");

var frequencyOption = new Option<int>(
    name: "--frequency",
    description: "Frequency of sending(ms).",
    getDefaultValue: () => 1000);
frequencyOption.AddAlias("-f");

var totalPacksOption = new Option<int>(
    name: "--total-packs",
    description: "Maximum number of packets sent. 0 is unlimited.",
    getDefaultValue: () => 0);
totalPacksOption.AddAlias("-tp");

var packSizeOption = new Option<int>(
    name: "--pack-size",
    description: "Package size.",
    getDefaultValue: () => 1024);
packSizeOption.AddAlias("-ps");

rootCommand.Add(typeOption);
rootCommand.Add(serverPortOption);
rootCommand.Add(serverIpOption);
rootCommand.Add(clientPortOption);
rootCommand.Add(compressedOption);
rootCommand.Add(frequencyOption);
rootCommand.Add(totalPacksOption);
rootCommand.Add(packSizeOption);

rootCommand.SetHandler(async (tester, serverPort, serverIp, clientPort, compressed, frequency, totalPacks, packSize) =>
    {
        tester.Compressed = compressed;
        tester.Frequency = frequency;
        tester.TotalPacks = totalPacks;
        tester.PackSize = packSize;

        var task1 = tester.RunServerAsync(serverPort);
        await Task.Delay(2000);
        var task2 = tester.RunClientAsync(clientPort, serverIp);
        await Task.WhenAll(task1, task2);
    },
    new SocketTesterBinder(typeOption),
    serverPortOption,
    serverIpOption,
    clientPortOption,
    compressedOption,
    frequencyOption,
    totalPacksOption,
    packSizeOption);

await rootCommand.InvokeAsync(args);

return returnCode;
