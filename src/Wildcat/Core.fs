namespace Wildcat.Core

open System


type Symbol = Symbol of string
type Exchange = Exchange of string
type Price = Price of float
type Quantity = Quantity of float
type DateTimeUtc = DateTimeUtc of DateTimeOffset


module PriceLevel =
    type T =
        { Price: Price
          Quantity: Quantity
          Count: int32 }

    /// <summary>
    /// Creates a price level.
    /// </summary>
    /// <param name="price">The price of the level.</param>
    /// <param name="quantity">The quantity of the level.</param>
    /// <param name="count">The count of the orders on the level.</param>
    let create price quantity count =
        { Price = price
          Quantity = quantity
          Count = count }

    /// <summary>
    /// Creates a price level with default values.
    /// </summary>
    let createDefault () =
        { Price = Price nan
          Quantity = Quantity 0.0
          Count = int32 0 }


module Messages =

    module TopOfBookMessage =
        // TODO: replace Symbol with InstrumentId and Exchange with ExchangeId
        type T =
            { Symbol: Symbol
              SequenceNumber: uint64
              TransactTime: DateTimeUtc
              Exchange: Exchange
              BestBid: PriceLevel.T option
              BestAsk: PriceLevel.T option }
