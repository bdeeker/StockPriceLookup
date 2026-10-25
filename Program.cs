var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// {ticker} is the route parameter. That part of the URL is passed into the string called ticker
app.MapGet("/api/price/{ticker}", (string ticker) =>
{
    // validation. Bad input gets a 400 Bad Request with a message before we do any work
    if (ticker.Length > 10 || !ticker.All(char.IsLetter))
    {
        return Results.BadRequest("A ticker is 1 to 10 letters, like MSFT or AAPL.");
    }

    // this is a record, a small read only class that holds data. ASP.NET Core turns it into JSON
    var result = new PriceResult(
        ticker.ToUpperInvariant(), // use ToUpperInvariant() for it to be correct in other languages
        123.45m, // testing with a fake price
        DateOnly.FromDateTime(DateTime.Today)
        );

    // reply with 200 OK and this data as JSON
    return Results.Ok(result);

});

app.Run();

// this is a declaration which comes after top level code in C#
record PriceResult(string Ticker, decimal Close, DateOnly Date);
