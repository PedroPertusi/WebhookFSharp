namespace Webhook

open System
open System.Threading.Tasks
open Microsoft.Data.Sqlite

module Db =

    /// Caminho para o arquivo SQLite
    let connectionString = "Data Source=webhook.db"

    /// Garante que a tabela exista (usado no startup)
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

    /// Retorna true se a transação já foi processada
    let hasProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT 1 FROM processed_transactions WHERE transaction_id = $id LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        use! reader = cmd.ExecuteReaderAsync()
        return reader.HasRows
    }

    /// Tenta marcar a transação como processada; retorna true se inseriu, false se já existia
    let markProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT OR IGNORE INTO processed_transactions (transaction_id) VALUES ($id);"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        let! rows = cmd.ExecuteNonQueryAsync()
        return rows > 0
    }
