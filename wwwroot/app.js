const form = document.getElementById("lookup-form");
const tickerInput = document.getElementById("ticker");
const result = document.getElementById("result");

form.addEventListener("submit", async (event) => {
    event.preventDefault(); // without this, the form does an old style submit and reloads the page

    const ticker = tickerInput.value.trim();
    result.textContent = "Loading...";

    try {
        // Relative URL: the page and the API come from the same site, so no CORS setup is needed (Cross-Origin Resource Sharing, browser security rule)
        // encodeURIComponent: if someone types a/b it is sent safely without breaking the URL. The server still rejects it with a 400
        const response = await fetch(`/api/price/${encodeURIComponent(ticker)}`); 

        // Errors come back as BadRequest and NotFound as a plain JSON string, Problem (from Program.cs) sends an object with the message
        if (!response.ok) {
            const error = await response.json();
            result.textContent = typeof error === "string" ? error : error.detail ?? "Something went wrong.";

            return;
        }

        const price = await response.json();
        result.textContent = `${price.ticker} closed at $${price.close.toFixed(2)} on ${price.date}.`;

    } catch {
        result.textContent = "Could not reach the server.";
    }
});