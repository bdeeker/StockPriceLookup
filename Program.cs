using System.Net;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Register the factory and reuse network connections so we don't run out of sockets under load
builder.Services.AddHttpClient();

var app = builder.Build();

// Makes a visit to / to serve index.html
app.UseDefaultFiles();

// Serve files from wwwroot folder
app.UseStaticFiles(); 

// {ticker} is the route parameter. That part of the URL is passed into the string called ticker
// async because calling Tiingo takes time
// the parameters IHttpClientFactory and IConfiguration are seen by .NET which passes them in. This is Dependency Injection
app.MapGet("/api/price/{ticker}", async (string ticker, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    // validation. Bad input gets a 400 Bad Request with a message before we do any work
    if (ticker.Length > 10 || !ticker.All(char.IsLetter))
    {
        return Results.BadRequest("A ticker is 1 to 10 letters, like MSFT or AAPL.");
    }

    var symbol = ticker.ToUpperInvariant();

    // Building the API call to Tiingo to get the daily prices for this symbol
    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.tiingo.com/tiingo/daily/{symbol}/prices");

    // Add the Tiingo token to the header, not the URL so it doesn't end up in the server logs
    request.Headers.Authorization = new AuthenticationHeaderValue("Token", config["Tiingo:ApiToken"]);

    var http = httpClientFactory.CreateClient();

    // await frees the server to handle other requests because it takes time to call Tiingo and have the response returned
    var response = await http.SendAsync(request);

    // Tiingo doesn't know the ticker
    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound($"No price found for {symbol}.");
    }

    // Tiingo is down or rejects the token
    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("The price service is not available right now.");
    }

    // Turns Tiingo's JSON into an array of TiingoPrice
    var prices = await response.Content.ReadFromJsonAsync<TiingoPrice[]>();

    // null/empty check Tiingo can return 200 OK with an empty list, so treat as not found
    if (prices is null || prices.Length == 0)
    {
        return Results.NotFound($"No price found for {symbol}.");
    }

    // ^1 means the last item, which is the most recent day
    var latest = prices[^1];

    var result = new PriceResult(symbol, latest.Close, DateOnly.FromDateTime(latest.Date));

    // reply with 200 OK and this data as JSON
    return Results.Ok(result);

});

app.Run();

// these are declaration which must come after top level code in C#
record PriceResult(string Ticker, decimal Close, DateOnly Date);
record TiingoPrice(DateTime Date, decimal Close);
