using System.CommandLine;
using System.CommandLine.Binding;

namespace SocketTester;

internal interface ISocketTester
{
    bool Compressed { get; set; }
    int Frequency { get; set; }
    int TotalPacks { get; set; }
    int PackSize { get; set; }
    Task RunServerAsync(int port = 9999);
    Task RunClientAsync(int port = 8888, string ip = "127.0.0.1");
}

internal class SocketTesterBinder : BinderBase<ISocketTester>
{
    private readonly Option<string> _typeOption;

    public SocketTesterBinder(Option<string> typeOption)
    {
        _typeOption = typeOption;
    }

    protected override ISocketTester GetBoundValue(BindingContext bindingContext)
    {
        var value = bindingContext.ParseResult.GetValueForOption(_typeOption);
        return value switch
        {
            "tcp" => new TcpTester(),
            "udp" => new UdpTester(),
            _ => new TcpTester()
        };
    }
}
