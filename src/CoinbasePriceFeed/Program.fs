open Wildcat.Core
open Wildcat.Coinbase

[<EntryPoint>]
let main argv =
    let productId_, sleepMillis_ =
        match (Array.length argv) with
        | 0 -> failwith "no args provided"
        | 1 -> argv[0], "5000"
        | 2 -> argv[0], argv[1]
        | _ -> failwithf "Unexpected args: %A" argv
    
    let productId = ProductId productId_
    let defaultSleepMillis = 5000
    let sleepMillis =
        match Parser.tryParseInt32 sleepMillis_ with
        | Some x -> x
        | None -> defaultSleepMillis

    let priceFeedSettings = CoinbasePriceFeed.Settings.create productId sleepMillis
    printfn "Running CoinbasePriceFeed with settings: %A" priceFeedSettings

    CoinbasePriceFeed.run priceFeedSettings (fun x -> printfn "onMessage: %A" x)

    0
