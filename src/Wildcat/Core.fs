namespace Wildcat


type InstrumentId = InstrumentId of uint64
type Symbol = Symbol of string
type Price = Price of float
type Quantity = Quantity of float


module Instrument =
    type T =
        { InstrumentId: InstrumentId
          Symbol: Symbol }

    let create instrumentId symbol =
        { InstrumentId = instrumentId
          Symbol = symbol }


module PriceLevel =
    type T =
        { Price: Price
          Quantity: Quantity
          Count: uint32 }

    let create price quantity count =
        { Price = price
          Quantity = quantity
          Count = count }

    let createDefault () =
        { Price = Price nan
          Quantity = Quantity 0.0
          Count = uint32 0 }
