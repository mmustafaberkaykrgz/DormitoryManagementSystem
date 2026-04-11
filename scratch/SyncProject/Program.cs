using Microsoft.Data.Sqlite;

string dbPath = @"C:\Users\Berkay\Desktop\WEB PROJE\DormitoryManagementSystem\Dormitory.db";
string oldName = "20260408100616_IlkKurulum";
string newName = "20260408100616_FirstSetup";

using (var connection = new SqliteConnection($"Data Source={dbPath}"))
{
    connection.Open();
    
    var command = connection.CreateCommand();
    command.CommandText = "UPDATE __EFMigrationsHistory SET MigrationId = @newName WHERE MigrationId = @oldName";
    command.Parameters.AddWithValue("@newName", newName);
    command.Parameters.AddWithValue("@oldName", oldName);
    
    int rows = command.ExecuteNonQuery();
    Console.WriteLine($"Database updated successfully. Rows affected: {rows}");
}
