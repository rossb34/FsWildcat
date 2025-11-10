# FsWildcat

FsWildcat is a F# library implementing a subset of the Coinbase REST API to write price feed applications.

Note: My primary intent of this library is a project to learn functional programming concepts and F#.

## Use Case
FsWildcat can be used by systems that require low to mid-frequency price data. This should not be used by higher 
frequency applications or trading platforms that require tick-level data.

## Example
```F#
open Wildcat.Core
open Wildcat.Coinbase

[<EntryPoint>]
let main argv =

    // Specify the product, e.g. BTC-USD
    let productId = ProductId "BTC-USD"

    // Specify the sleep interval in milliseconds. This is the 
    let sleepMillis = 5000
    
    // Create the price feed settings record.
    let priceFeedSettings = CoinbasePriceFeed.Settings.create productId sleepMillis
    printfn "Running CoinbasePriceFeed with settings: %A" priceFeedSettings

    // Here we define a simple lambda as the message handler callback.
    // A typical product use case for the message handler would define a function to write to a log, message queue, or publish to a db.
    CoinbasePriceFeed.run priceFeedSettings (fun x -> printfn "onMessage: %A" x)

    // Return 0 to indicate success
    0
```