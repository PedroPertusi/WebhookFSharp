namespace Webhook

open System
open System.Text.Json
open Microsoft.AspNetCore.Http

module Validation =

    let parsePayload (body: string) : Payload option =
        try
            JsonSerializer.Deserialize<Payload>(body) |> Some
        with _ ->
            None

    let validatePayload (payload: Payload) : bool =
        payload.event = "payment_success"
        && payload.amount > 0.0
        && not (String.IsNullOrWhiteSpace payload.transactionId)
        && not (String.IsNullOrWhiteSpace payload.currency)
        && payload.timestamp > DateTime.MinValue

    let parseAndValidate (body: string) : Payload option =
        let parsed = parsePayload body

        match parsed with
        | Some p ->
            let isValid = validatePayload p
            if isValid then
                Some p
            else
                None

        | None ->
            None

    let mutable expectedToken = ""

    let validateToken (ctx: HttpContext) : bool =
        match ctx.Request.Headers.TryGetValue "X-Webhook-Token" with
        | true, values -> values.Count > 0 && values.[0] = expectedToken
        | _ -> false
