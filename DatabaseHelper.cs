using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public static class DatabaseHelper
{
    private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public static DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
    {
        int maxRetries = 3;
        int delayMs = 10; // 1 second delay between retries

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    var dt = new DataTable();
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                // Only retry on specific exceptions (e.g., timeout)
                if (ex.InnerException != null && ( ex.InnerException.Message.Contains("timeout") || ex.InnerException.Message.Contains("timed out")))
                {
                    if (attempt == maxRetries)
                        throw; // rethrow if last attempt

                    System.Threading.Thread.Sleep(delayMs); // wait before retrying
                }
                else
                {
                    throw ex; // rethrow for other exceptions
                }
            }
        }
        // Should never reach here
        throw new Exception("Failed to execute stored procedure after retries.");
    }

    public static int ExecuteScalarStoredProcedure(string procedureName, params SqlParameter[] parameters)
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}