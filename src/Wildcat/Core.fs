namespace Wildcat


type Price = Price of float
type Quantity = Quantity of float


module PriceLevel =
    type T =
        { Price: Price
          Quantity: Quantity
          Count: uint32 }

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
          Count = uint32 0 }
