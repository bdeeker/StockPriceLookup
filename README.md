# StockPriceLookup
A simple web app that looks up the most recent closing price of a US stock using Tiingo's free API.

URL is http://stockpricelookup.runasp.net/

## How it works
The user enters a ticker symbol such as AAPL and clicks Look up. app.js sends a GET request with fetch to /api/price/{ticker}. ASP.NET Core routes it to the MapGet endpoint in Program.cs, which checks the ticker is 1 to 10 letters, calls Tiingo with the API token, and returns just the ticker, closing price and date as JSON. app.js shows the result on the page, or the error message if something went wrong.

## API 
`GET /api/price/{ticker}`

Example `GET /api/price/KO` returns

```json
{ "ticker": "KO", "close": 70.12, "date": "2026-10-24" }
```

| Status | Meaning |
| --- | --- |
| 200 | The price was found |
| 400 | The ticker format is wrong (it must be 1 to 10 letters) |
| 404 | Tiingo doesn't know the ticker |
| 500 | Tiingo is not available, or rejected the API token |

## Tech stack
- .NET 10, ASP.NET Core Minimal API
- IHttpClientFactory for the call to Tiingo
- HTML, CSS and plain JavaScript (fetch), served from wwwroot
- Tiingo end-of-day prices API

## Run it locally
1. Get a free API token at tiingo.com.
2. In Visual Studio, right-click the project, choose Manage User Secrets, and add:
   ```json
   { "Tiingo": { "ApiToken": "your-token" } }
   ```
3. Press F5 and look up a ticker.

## Decisions
- The server validates the ticker, even though the page limits the input. Anyone can call the API without using the page.
- The Tiingo token is sent in a request header, not the URL, so it doesn't end up in logs. It's kept in User Secrets, never in git.
- IHttpClientFactory reuses network connections, so the app doesn't run out of sockets under load.
- Prices use decimal, not double, because money needs exact values.
- The page shows results with textContent, not innerHTML, so nothing returned can run as code (prevents XSS).
- The API returns its own small JSON shape instead of passing Tiingo's response through. The page doesn't depend on Tiingo's format, so the price source could change without breaking it.

