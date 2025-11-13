open Wildcat.CoinbaseIntx
open Wildcat.Core

[<EntryPoint>]
let main argv =

    let instrumentId_, sleepMillis_ =
        match (Array.length argv) with
        | 0 -> failwith "no args provided"
        | 1 -> argv[0], "5000"
        | 2 -> argv[0], argv[1]
        | _ -> failwithf "Unexpected args: %A" argv
    
    let instrumentId = InstrumentId instrumentId_
    let defaultSleepMillis = 5000
    let sleepMillis =
        match Parser.tryParseInt32 sleepMillis_ with
        | Some x -> x
        | None -> defaultSleepMillis

    let settings = CoinbaseIntxPriceFeed.Settings.create instrumentId sleepMillis

    CoinbaseIntxPriceFeed.run settings (fun x -> printfn "onMessage: %A" x)

    0
