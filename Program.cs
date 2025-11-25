using Neodynamic.SDK.Printing;
using System.Net;
using System.Net.Sockets;
using System.Text;

// EASY INSTALL TTF FONT TO ZEBRA PRINTER => HOW TO DO IT WITH NOVEXX PRINTERS?
bool doUploadFont = false;
if (doUploadFont)
{
    Socket? socketFont = null;
    NetworkStream? streamFont = null;
    try
    {
        string name = "E:TACOS.TTF";
        var data = BitConverter.ToString(System.IO.File.ReadAllBytes("TACOS.TTF")).Replace("-", "");
        var fileSize = System.IO.File.ReadAllBytes("TACOS.TTF").Length;
        var zplFont = $@"^XA^CI~DU{name},{fileSize},{data}^XZ";
        IPEndPoint endpointFont = new IPEndPoint(Dns.GetHostEntry("10.10.1.69").AddressList[0]!, 9100);
        socketFont = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socketFont.Connect(endpointFont);
        streamFont = new NetworkStream(socketFont);
        byte[] fontBytes = Encoding.ASCII.GetBytes(zplFont);
        streamFont.Write(fontBytes, 0, fontBytes.Length);
        Console.WriteLine($"Font uploaded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending to printer: {ex.Message}");
    }
    finally
    {
        streamFont?.Close();
        if (socketFont?.Connected == true)
            socketFont.Close();
    }
}

//CREATE LABEL
ThermalLabel tLabel = new ThermalLabel(UnitType.Mm, 75, 50);
tLabel.Items.Add(new BarcodeItem()
{
    Symbology = BarcodeSymbology.Ean13,
    DataField = nameof(Label.Ean13),
    X = 2.0,
    Y = 2.0,
    Width = tLabel.Width - 5,
    Height = 13,
    BarHeight = 14,
    BarWidth = 0.5,
    EanUpcGuardBarHeight = 7,
    PrintAsResidentElement = true
});
tLabel.Items.Add(new TextItem
{
    DataField = nameof(Label.Name),
    X = 2,
    Y = 20,
    Width = tLabel.Width - 5,
    Height = 10,
    Font = { Name = "", Size = 24, CodePage = CodePage.UTF8, NameAtPrinterStorage = "E:TACOS.TTF" },
});


// LABEL DATA
tLabel.DataSource = new List<Label> { new() { Name = "TACOS 1", Ean13 = "460566400005" }, new() { Name = "TACOS 2", Ean13 = "460566400005" } };
var pj = new PrintJob()
{
    Dpi = 203,
    ProgrammingLanguage = ProgrammingLanguage.ZPL
};


// GENERATE ZPL AND SAVE TO FILE
string zpl = pj.GetNativePrinterCommands(tLabel);
Console.WriteLine(zpl);
var outBytes = Encoding.UTF8.GetBytes(zpl);
System.IO.File.WriteAllBytes("printstream.zpl", outBytes);


//SEND TO ZEBRA PRINTER
bool doPrint = false;
if (doPrint)
{
    Socket? socket = null;
    NetworkStream? stream = null;
    IPEndPoint endpoint = new IPEndPoint(Dns.GetHostEntry("10.10.1.69").AddressList[0]!, 9100);
    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    socket.Connect(endpoint);
    stream = new NetworkStream(socket);
    stream.Write(outBytes, 0, outBytes.Length);
}

public class Label
{
    public required string Name { get; set; }
    public required string Ean13 { get; set; }
}
