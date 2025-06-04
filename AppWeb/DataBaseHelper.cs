using MySql.Data.MySqlClient;
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
            using (var con = new MySqlConnection(connectionString))
            using (var cmd = new MySqlCommand(query, con))
            using (var da = new MySqlDataAdapter(cmd))
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
            using (var con = new MySqlConnection(connectionString))
            using (var cmd = new MySqlCommand(query, con))
            using (var da = new MySqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
            if (dt.Rows.Count > 0)
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
    public int InsertRowReturnId(string tableName, Dictionary<string, object> data)
    {
        if (string.IsNullOrWhiteSpace(tableName) || data == null || data.Count == 0)
            return -1;

        var columns = string.Join(",", data.Keys.Select(k => $"`{k}`"));
        var parameters = string.Join(",", data.Keys.Select(k => "@" + k));
        var query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({parameters}); SELECT LAST_INSERT_ID();";

        try
        {
            using (var con = new MySqlConnection(connectionString))
            using (var cmd = new MySqlCommand(query, con))
            {
                foreach (var item in data)
                    cmd.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

                con.Open();
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en InsertRowReturnId: " + ex.Message);
            return -1;
        }
    }

    // ➕ Insertar fila usando diccionario
    public bool InsertRow(string tableName, Dictionary<string, object> data)
    {
        if (string.IsNullOrWhiteSpace(tableName) || data == null || data.Count == 0)
            return false;

        var columns = string.Join(",", data.Keys.Select(k => $"`{k}`"));
        var parameters = string.Join(",", data.Keys.Select(k => "@" + k));
        var query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({parameters})";

        try
        {
            using (var con = new MySqlConnection(connectionString))
            using (var cmd = new MySqlCommand(query, con))
            {
                foreach (var item in data)
                    cmd.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en InsertRow: " + ex.Message);
            return false;
        }
    }

    // ✏️ Actualizar fila usando diccionario
    public bool UpdateRow(string tableName, Dictionary<string, object> data, string whereClause)
    {
        var setClause = string.Join(", ", data.Keys.Select(k => $"`{k}` = @{k}"));
        var query = $"UPDATE `{tableName}` SET {setClause} WHERE {whereClause}";

        using (var con = new MySqlConnection(connectionString))
        using (var cmd = new MySqlCommand(query, con))
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
        var query = $"DELETE FROM `{tableName}` WHERE {whereClause}";

        using (var con = new MySqlConnection(connectionString))
        using (var cmd = new MySqlCommand(query, con))
        {
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
