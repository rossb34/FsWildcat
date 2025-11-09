namespace Wildcat.Coinbase

/// The Coinbase ProductId, e.g. BTC-USD
type ProductId = ProductId of string

type BookLevel =
    | LevelOne = 1
    | LevelTwo = 2
    | LevelThree = 3


/// Implements a subset of the Coinbase REST API
module CoinbaseRest =
    open Wildcat.Net
    open Wildcat.Core
    open System.Text.Json

    /// The Coinbase REST API base url.
    let baseUrl = "https://api.exchange.coinbase.com"

    /// Coinbase exchange name
    let exchange = Exchange "Coinbase"


    /// <summary>
    /// Sends an asynchronous request to get the product book snapshot of the specified product.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="productId">The product identifier.</param>
    /// <param name="level">The book level.</param>
    /// <returns>Returns an <c>Ok</c> result with the response if the request was successful, otherwise an <c>Error</c> with the HTTP status code and response content</returns>
    let getProductBookAsync httpClient productId (level: BookLevel) =
        async {
            let bookLevel = int level
            let (ProductId productId) = productId

            let url =
                baseUrl
                + "/products/"
                + productId
                + "/book?level="
                + (string bookLevel)

            let! result = Http.getAsync httpClient url
            return result
        }


    let private parseTime time =
        // Parse the Coinbase time string to a DateTimeUtc.
        // The Coinbase time is an ISO 8601 formatted string with nanos. Example: "2025-10-28T02:50:43.768106912Z".
        // Drop the nanos due to Windows resolution that only supports a precision of 100 nanos.
        let eventTimeMicros = Parser.tryDropNanos time

        match eventTimeMicros with
        | Some x -> Parser.tryParseDateTimeUtcMicros x
        | None -> None


    let private getPriceLevelFromArray (elem: JsonElement) =
        // Parses a product book entry array with 3 elements to a PriceLevel.
        // The product book entry is an array with 3 elements of [price, quantity, count]. Example: ["113729.4","0.28534551",5]
        let px = elem[ 0 ].GetString() |> Parser.tryParseFloat
        let qty = elem[ 1 ].GetString() |> Parser.tryParseFloat
        let count = elem[ 2 ].GetInt32()

        match (px, qty) with
        | (Some px, Some qty) -> Some(PriceLevel.create (Price px) (Quantity qty) count)
        | _ -> None


    let private processProductBookResponse (response: string) (symbol: Symbol) (exchange: Exchange) =
        // Process the json product book response to a TopOfBook.T
        // Example: {"bids":[["113729.4","0.28534551",5]],"asks":[["113729.41","0.00096729",2]],"sequence":114340891054,"auction_mode":false,"auction":null,"time":"2025-10-28T02:50:43.768106912Z"}
        use jdoc = JsonDocument.Parse(response)
        let elem = jdoc.RootElement

        // Get the sequence number
        let seqNum = elem.GetProperty("sequence").GetUInt64()

        // Get the event time
        let coinbaseTime = elem.GetProperty("time").GetString()
        let time = parseTime coinbaseTime

        // Get the best bid
        let bids = elem.GetProperty("bids")

        let bestBid =
            if bids.ValueKind = JsonValueKind.Array
               && bids.GetArrayLength() > 0 then
                getPriceLevelFromArray bids[0]
            else
                None

        // Get the bst ask
        let asks = elem.GetProperty("asks")

        let bestAsk =
            if asks.ValueKind = JsonValueKind.Array
               && asks.GetArrayLength() > 0 then
                getPriceLevelFromArray asks[0]
            else
                None

        match time with
        | Some transactTime ->
            // Create a TopOfBook
            Some(TopOfBook.create symbol transactTime seqNum exchange bestBid bestAsk)
        | None ->
            // TODO: how can I know the time parsing failed?
            printfn "Failed to parse timestamp %s" coinbaseTime
            None


    let private symbolFromProductId productId =
        // Cast from ProductId -> Symbol
        let (ProductId product_) = productId
        Symbol product_


    /// <summary>
    /// Recursively polls the product book endpoint for L1 (i.e. top of book) book snapshots.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="productId">The product identifier.</param>
    /// <param name="sleepMillis">The milliseconds to sleep.</param>
    /// <param name="onMessage">The function callback is called when the response is processed.</param>
    let rec pollProductBookAsync httpClient productId (sleepMillis: int) onMessage =
        async {
            let! result = getProductBookAsync httpClient productId BookLevel.LevelOne

            match result with
            | Ok response ->
                let symbol = symbolFromProductId productId
                let tob = processProductBookResponse response symbol exchange

                match tob with
                | Some tob_ -> onMessage tob_
                | None -> ()
            | Error (statusCode, message) ->
                // some useful error message from result
                // onMessage message
                printfn "error: %A %s" statusCode message
                ()

            do! Async.Sleep sleepMillis
            return! pollProductBookAsync httpClient productId sleepMillis onMessage
        }


module CoinbasePriceFeed =
    open System.Net.Http
    open System.Net.Http.Headers

    module Settings =
        type T =
            { ProductId: ProductId
              SleepMillis: int }

        /// <summary>
        /// Creates the Coinbase price feed settings.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="sleepMillis">The milliseconds to sleep.</param>
        let create productId sleepMillis =
            { ProductId = productId
              SleepMillis = sleepMillis }


    /// <summary>
    /// Runs the Coinbase price feed.
    /// </summary>
    /// <param name="settings">The price feed settings.</param>
    /// <param name="onMessage">The function callback to handle messages from the price feed.</param>
    let run (settings: Settings.T) onMessage =
        use httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue("CoinbasePriceFeed", "0.1.0"))
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json")

        CoinbaseRest.pollProductBookAsync httpClient settings.ProductId settings.SleepMillis onMessage
        |> Async.RunSynchronously
