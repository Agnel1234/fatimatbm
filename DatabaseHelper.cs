using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public static class DatabaseHelper
{
    private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public static DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
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
}