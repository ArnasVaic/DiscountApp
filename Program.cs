
if(args.Length is not 1 || args[0] is "--help")
{
    Console.Error.WriteLine("usage: vinted-hw <file>");
    return;
}

var filename = args[0];

try {
    var file = File.ReadAllText(filename);

}
catch(Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
}