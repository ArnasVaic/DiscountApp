using DiscountApp.Driver;

var result = Driver
    .WithArgs(args)
    .Bind(driver => driver.Run());

if(result.IsFailure)
{
    Console.Error.WriteLine($"Error: {result.Error}.");
}