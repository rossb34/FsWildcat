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


module TopOfBook =
    // TODO: replace Symbol with InstrumentId and Exchange with ExchangeId
    type T =
        { Symbol: Symbol
          TransactTime: DateTimeUtc
          SequenceNumber: uint64
          Exchange: Exchange
          BestBid: PriceLevel.T option
          BestAsk: PriceLevel.T option }


    /// <summary>
    /// Creates a ``TopOfBook``.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="transactTime">The transact time.</param>
    /// <param name="seqNum">The sequence number.</param>
    /// <param name="exchange">The exchange.</param>
    /// <param name="bestBid">The best bid price level.</param>
    /// <param name="bestAsk">The best ask price level.</param>
    let create symbol transactTime seqNum exchange bestBid bestAsk =
        { Symbol = symbol
          TransactTime = transactTime
          SequenceNumber = seqNum
          Exchange = exchange
          BestBid = bestBid
          BestAsk = bestAsk }
