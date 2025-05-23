namespace Webhook

open System
open System.Threading.Tasks
open Microsoft.Data.Sqlite

/// Módulo de acesso ao banco de dados SQLite para controle de transações processadas.
module Db =
    /// Connection string para o arquivo SQLite 'webhook.db'.
    let connectionString = "Data Source=webhook.db"

    /// Garante que a tabela 'processed_transactions' exista no banco de dados.
    /// Cria a tabela se ela ainda não existe, com colunas:
    /// - transaction_id: chave primária (TEXT)
    /// - processed_at: timestamp da inserção (padrão CURRENT_TIMESTAMP)
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

    /// Verifica se uma transação já foi processada.
    /// <param name="transactionId">ID da transação a ser verificada.</param>
    /// <returns>Task que resulta em 'true' se existir registro, 'false' caso contrário.</returns>
    let hasProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT 1 FROM processed_transactions WHERE transaction_id = $id LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        use! reader = cmd.ExecuteReaderAsync()
        return reader.HasRows
    }

    /// Marca uma transação como processada no banco de dados.
    /// Se o registro já existir, não lança erro (usa INSERT OR IGNORE).
    /// <param name="transactionId">ID da transação a ser marcada.</param>
    /// <returns>Task que resulta em 'true' se inseriu um novo registro, 'false' se já existia.</returns>
    let markProcessed (transactionId: string) : Task<bool> = task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync()
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT OR IGNORE INTO processed_transactions (transaction_id) VALUES ($id);"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        let! rows = cmd.ExecuteNonQueryAsync()
        return rows > 0
    }
