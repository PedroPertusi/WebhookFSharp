namespace Webhook

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks

module WebhookClient = 

    let client = new HttpClient()

    let private postJsonAsync (url: string) (payload: obj) : Task<HttpResponseMessage> =
        let json = JsonSerializer.Serialize(payload)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        client.PostAsync(url, content)

    let postConfirm (transactionId: string) : Task<HttpResponseMessage> =
        postJsonAsync "http://localhost:5001/confirmar" {| transaction_id = transactionId |}

    let postCancel (transactionId: string) : Task<HttpResponseMessage> =
        postJsonAsync "http://localhost:5001/cancelar" {| transaction_id = transactionId |}