namespace Wildcat.Net


module Http =
    open System.Net.Http


    /// <summary>
    /// Sends an asynchronous GET request to the specified url.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="url">The url the request is sent to.</param>
    /// <returns>Returns an <c>Ok</c> result with the response if the request was successful, otherwise an <c>Error</c> with the HTTP status code and response content</returns>
    let getAsync (client: HttpClient) (url: string) =
        async {
            let! response = client.GetAsync(url) |> Async.AwaitTask

            let! content =
                response.Content.ReadAsStringAsync()
                |> Async.AwaitTask

            return
                if response.IsSuccessStatusCode then
                    Ok content
                else
                    Error(response.StatusCode, content)
        }
