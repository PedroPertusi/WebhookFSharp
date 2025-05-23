namespace Webhook

open System
open System.Text.Json.Serialization

/// Representa o formato do payload JSON recebido pelo webhook.
type Payload = {
  [<JsonPropertyName("event")>]
  event: string

  [<JsonPropertyName("transaction_id")>]
  transactionId: string

  [<JsonPropertyName("amount")>]
  [<JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)>]
  amount: float

  [<JsonPropertyName("currency")>]
  currency: string

  [<JsonPropertyName("timestamp")>]
  timestamp: DateTime
}