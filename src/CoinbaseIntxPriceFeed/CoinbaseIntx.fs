namespace Wildcat.CoinbaseIntx

/// The CoinbaseIntx InstrumentId, e.g. BTC-PERP
type InstrumentId = InstrumentId of string

module CoinbaseIntxRest =
    open Wildcat.Net
    open Wildcat.Core
    open System.Text.Json


    /// The Coinbase Intx REST API base url.
    let baseUrl = "https://api.international.coinbase.com/api/v1"


    /// Coinbase exchange name
    let exchange = Exchange "CoinbaseIntx"


    /// <summary>
    /// Sends an asynchronous request to get the product book snapshot of the specified instrument.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="instrumentId">The instrument identifier.</param>
    /// <returns>Returns an <c>Ok</c> result with the response if the request was successful, otherwise an <c>Error</c> with the HTTP status code and response content</returns>
    let getInstrumentQuoteAsync httpClient instrumentId =
        async {
            let (InstrumentId instrument) = instrumentId

            let url = baseUrl + "/instruments/" + instrument + "/quote"

            let! result = Http.getAsync httpClient url
            return result
        }
    
    let private tryParseTimestamp time =
        // The Coinbase Intx timestamp is an iso 8601 formatted string with millisecond precision. Example: "2025-11-11T12:45:23.157Z"
        Parser.tryParseDateTimeUtc time Constants.DATETIME_MILLIS_FMT


    let private processQuoteResponse (response: string) (symbol: Symbol) =
        use jdoc = JsonDocument.Parse(response)
        let elem = jdoc.RootElement

        let bestBidPrice =
            elem.GetProperty("best_bid_price").GetString()
            |> Parser.tryParseFloat

        let bestBidSize =
            elem.GetProperty("best_bid_size").GetString()
            |> Parser.tryParseFloat

        let bestBid =
            match (bestBidPrice, bestBidSize) with
            | (Some price, Some quantity) -> Some(PriceLevel.create (Price price) (Quantity quantity) 1)
            | _ -> None

        let bestAskPrice =
            elem.GetProperty("best_ask_price").GetString()
            |> Parser.tryParseFloat

        let bestAskSize =
            elem.GetProperty("best_ask_size").GetString()
            |> Parser.tryParseFloat

        let bestAsk =
            match (bestAskPrice, bestAskSize) with
            | (Some price, Some quantity) -> Some(PriceLevel.create (Price price) (Quantity quantity) 1)
            | _ -> None
        
        // Parse the timestamp example: {"timestamp":"2025-11-11T12:45:23.157Z"}
        let coinbaseTime = elem.GetProperty("timestamp").GetString()
        let timestamp = tryParseTimestamp coinbaseTime
        
        // There is no sequence number in the instrument quote response.
        let seqNum = Constants.UINT64_NULL

        match timestamp with
        | Some transactTime ->
            // Create a TopOfBook
            Some(TopOfBook.create symbol transactTime seqNum exchange bestBid bestAsk)
        | None ->
            // TODO: how can I know the time parsing failed?
            printfn "Failed to parse timestamp %s" coinbaseTime
            None
    

    let private symbolFromInstrumentId instrumentId =
        // Cast from InstrumenttId -> Symbol
        let (InstrumentId instrument_) = instrumentId
        Symbol instrument_
    
    /// <summary>
    /// Recursively polls the instrument quote endpoint for L1 (i.e. top of book) book snapshots.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="instrumentId">The instrument name.</param>
    /// <param name="sleepMillis">The milliseconds to sleep.</param>
    /// <param name="onMessage">The function callback is called when the response is processed.</param>
    let rec pollInstrumentQuoteAsync httpClient instrumentId (sleepMillis: int) onMessage =
        async {
            let! result = getInstrumentQuoteAsync httpClient instrumentId

            match result with
            | Ok response ->
                let symbol = symbolFromInstrumentId instrumentId
                let tob = processQuoteResponse response symbol

                match tob with
                | Some tob_ -> onMessage tob_
                | None -> ()
            | Error (statusCode, message) ->
                // some useful error message from result
                // onMessage message
                printfn "error: %A %s" statusCode message
                ()

            do! Async.Sleep sleepMillis
            return! pollInstrumentQuoteAsync httpClient instrumentId sleepMillis onMessage
        }

module CoinbaseIntxPriceFeed =
    open System.Net.Http
    open System.Net.Http.Headers

    module Settings =
        type T =
            { InstrumentId: InstrumentId
              SleepMillis: int }

        /// <summary>
        /// Creates the Coinbase International price feed settings.
        /// </summary>
        /// <param name="instrumentId">The instrument name.</param>
        /// <param name="sleepMillis">The milliseconds to sleep.</param>
        let create instrumentId sleepMillis =
            { InstrumentId = instrumentId
              SleepMillis = sleepMillis }


    /// <summary>
    /// Runs the Coinbase price feed.
    /// </summary>
    /// <param name="settings">The price feed settings.</param>
    /// <param name="onMessage">The function callback to handle messages from the price feed.</param>
    let run (settings: Settings.T) onMessage =
        use httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue("CoinbaseIntxPriceFeed", "0.1.0"))
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json")

        CoinbaseIntxRest.pollInstrumentQuoteAsync httpClient settings.InstrumentId settings.SleepMillis onMessage
        |> Async.RunSynchronously
