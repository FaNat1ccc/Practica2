using System;
using System.Data;
using System.Data.Common;

namespace DataTableStates
{
    class Program
    {
        static void Main()
        {
            DataTable table = new DataTable("Students");
            table.Columns.Add("student_id", typeof(int));
            table.Columns.Add("first_name", typeof(string));
            table.Columns.Add("last_name", typeof(string));
            table.Columns.Add("group_number", typeof(string));

            try
            {
                string connString = "Host=localhost;Port=5432;Database=university;Username=postgres;Password=postgres";
                DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");

                using (DbConnection connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connString;
                    connection.Open();

                    DbCommand command = factory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT student_id, first_name, last_name, group_number FROM students";

                    DbDataAdapter adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    adapter.Fill(table);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось подключиться к БД: {ex.Message}");
                Console.WriteLine("Используются тестовые данные.\n");

                table.Rows.Add(1, "Алексей", "Иванов", "ИТ-101");
                table.Rows.Add(2, "Никита", "Смирнов", "ИТ-102");
                table.Rows.Add(3, "Виктор", "Сидоров", "ИТ-103");
            }

            table.AcceptChanges();

            Console.WriteLine(" Исходная таблица ");
            PrintTable(table);

            Console.WriteLine("\n Изменение имени первого студента ");
            table.Rows[0]["first_name"] = "Александр";
            Console.WriteLine($"Изменено: {table.Rows[0]["first_name", DataRowVersion.Original]} → {table.Rows[0]["first_name", DataRowVersion.Current]}");
            PrintRowState(table.Rows[0]);

            Console.WriteLine("\n Добавление нового студента ");
            DataRow addedRow = table.NewRow();
            addedRow["student_id"] = 999;
            addedRow["first_name"] = "Мария";
            addedRow["last_name"] = "Иванова";
            addedRow["group_number"] = "ИТ-201";
            table.Rows.Add(addedRow);
            Console.WriteLine($"Добавлен: {addedRow["first_name"]} {addedRow["last_name"]}");
            PrintRowState(addedRow);

            Console.WriteLine("\n Удаление второго студента ");
            DataRow deletedRow = table.Rows[1];
            Console.WriteLine($"Удаляется: {deletedRow["first_name"]} {deletedRow["last_name"]}");
            deletedRow.Delete();
            PrintRowState(deletedRow);

            Console.WriteLine("\n Таблица после всех изменений ");
            PrintTable(table);

            Console.WriteLine("\n Журнал изменений ");
            foreach (DataRow row in table.Rows)
            {
                switch (row.RowState)
                {
                    case DataRowState.Modified:
                        string originalName = row["first_name", DataRowVersion.Original].ToString();
                        string currentName = row["first_name", DataRowVersion.Current].ToString();
                        Console.WriteLine($"[MODIFIED] {originalName} → {currentName} {row["last_name"]}");
                        break;

                    case DataRowState.Added:
                        Console.WriteLine($"[ADDED]    {row["first_name", DataRowVersion.Current]} {row["last_name", DataRowVersion.Current]}");
                        break;

                    case DataRowState.Deleted:
                        Console.WriteLine($"[DELETED]  {row["first_name", DataRowVersion.Original]} {row["last_name", DataRowVersion.Original]}");
                        break;
                }
            }

            Console.WriteLine("\n Вызов RejectChanges() ");
            table.RejectChanges();
            PrintTable(table);
        }

        static void PrintTable(DataTable table)
        {
            Console.WriteLine($"Количество строк: {table.Rows.Count}");
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    Console.WriteLine($"ID: {row["student_id"]}, Имя: {row["first_name"]}, Фамилия: {row["last_name"]}, Группа: {row["group_number"]}");
                }
                else
                {
                    Console.WriteLine($"[УДАЛЕНА] ID: {row["student_id", DataRowVersion.Original]}, Имя: {row["first_name", DataRowVersion.Original]}, Фамилия: {row["last_name", DataRowVersion.Original]}");
                }
            }
        }

        static void PrintRowState(DataRow row)
        {
            Console.WriteLine($"  Состояние строки: {row.RowState}");
        }
    }
}