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


module Parser =
    open System.Globalization


    /// <summary>
    /// Parses the string representation of a number to a double-precision floating-point number.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseFloat (s: string) =
        let success, value = Double.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a number to a 32-bit signed integer.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseInt32 (s: string) =
        let success, value = Int32.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a number to a 64-bit signed integer.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseInt64 (s: string) =
        let success, value = Int64.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a number to a 32-bit unsigned integer.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseUInt32 (s: string) =
        let success, value = UInt32.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a number to a 64-bit signed integer.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseUInt64 (s: string) =
        let success, value = UInt64.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a logical value to a boolean.
    /// </summary>
    /// <param name="s">A string of the value to parse.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseBool (s: string) =
        let success, value = Boolean.TryParse s

        match success with
        | true -> Some value
        | false -> None


    /// <summary>
    /// Parses the string representation of a timestamp to a ``DateTimeUtc``.
    /// </summary>
    /// <param name="s">A string of the time to parse. The input string must be an ISO 601 formatted timestamp with microsecond precision.</param>
    /// <returns>Returns <c>Some</c> with the value if successful otherwise <c>None</c>.</returns>
    let tryParseDateTimeUtcMicros (s: string) =
        let fmt = "yyyy-MM-ddTHH:mm:ss.ffffffZ"

        let success, value = DateTimeOffset.TryParseExact(s, fmt, null, DateTimeStyles.None)

        match success with
        | true -> Some(DateTimeUtc value)
        | false -> None


    /// <summary>
    /// Drops the nanos portion from the time string.
    /// </summary>
    /// <param name="s">The input time string</param>
    /// <remarks>
    /// This function expects an input time string in ISO 8601 format with nanosecond precision.
    /// Example: 2025-10-28T02:50:49.965682759Z -> 2025-10-28T02:50:49.965682Z
    /// </remarks>
    /// <returns>Returns <c>Some</c> with the  time string with microsecond precision, otherwise <c>None</c></returns>
    let tryDropNanos s =
        let len = String.length s

        match len with
        | 30 -> Some(s[0..25] + "Z")
        | 27 -> Some s
        | _ -> None
