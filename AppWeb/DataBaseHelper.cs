using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public class DataBaseHelper
{
    private readonly string connectionString;

    public DataBaseHelper()
    {
        connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;
    }

    // 🔎 SELECT como DataTable
    public DataTable SelectTable(string query)
    {
        var dt = new DataTable();
        try
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en SelectTable: " + ex.Message);
        }
        return dt;
    }
    public bool Exists(string query)
    {
        var dt = new DataTable();
        bool existe = false;
        try
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
            if(dt.Rows.Count>0)
                {
                existe = true;
            }

        }
        catch (Exception ex)
        {
            return false;
        }
        return existe;
    }

    // ➕ Insertar fila usando diccionario
    public bool InsertRow(string tableName, Dictionary<string, object> data)
    {
        var columns = string.Join(",", data.Keys);
        var parameters = string.Join(",", data.Keys.Select(k => "@" + k));
        var query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

        using (var con = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand(query, con))
        {
            foreach (var item in data)
                cmd.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // ✏️ Actualizar fila usando diccionario
    public bool UpdateRow(string tableName, Dictionary<string, object> data, string whereClause)
    {
        var setClause = string.Join(", ", data.Keys.Select(k => $"{k} = @{k}"));
        var query = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

        using (var con = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand(query, con))
        {
            foreach (var item in data)
                cmd.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // ❌ Eliminar fila
    public bool DeleteRow(string tableName, string whereClause)
    {
        var query = $"DELETE FROM {tableName} WHERE {whereClause}";

        using (var con = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand(query, con))
        {
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
