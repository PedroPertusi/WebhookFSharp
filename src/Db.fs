namespace Webhook

open System
open System.Threading.Tasks
open Microsoft.Data.Sqlite

module Db =

    let connectionString = "Data Source=webhook.db"

    let ensureTable () =
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- """
            CREATE TABLE IF NOT EXISTS processed_transactions (
                transaction_id TEXT PRIMARY KEY,
                processed_at TEXT DEFAULT CURRENT_TIMESTAMP
            );
        """
        cmd.ExecuteNonQuery() |> ignore

    let hasProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT 1 FROM processed_transactions WHERE transaction_id = $id LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        use! reader = cmd.ExecuteReaderAsync()
        return reader.HasRows
    }

    let markProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT OR IGNORE INTO processed_transactions (transaction_id) VALUES ($id);"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        let! rows = cmd.ExecuteNonQueryAsync()
        return rows > 0
    }
