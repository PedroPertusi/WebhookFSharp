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
        printfn "Recebido payload: %A" payload

        payload.event = "payment_success"
        && payload.amount > 0.0
        && not (String.IsNullOrWhiteSpace payload.transactionId)
        && not (String.IsNullOrWhiteSpace payload.currency)
        && payload.timestamp > DateTime.MinValue

    let parseAndValidate (body: string) : Payload option =
        // printfn "Recebido payload: %s" body

        let parsed = parsePayload body
        // printfn "Resultado do parse: %A" parsed

        match parsed with
        | Some p ->
            let isValid = validatePayload p
            // printfn "Resultado da validação: %b" isValid
            if isValid then
                Some p
            else
                None

        | None ->
            None


    let expectedToken = "meu-token-secreto"

    let validateToken (ctx: HttpContext) : bool =
        match ctx.Request.Headers.TryGetValue "X-Webhook-Token" with
        | true, values -> values.Count > 0 && values.[0] = expectedToken
        | _ -> false
