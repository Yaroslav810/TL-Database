using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogApp
{
    class Program
    {
        private static string _connectionString = @"Server=MSI\SQLEXPRESS;Database=Shop;Trusted_Connection=True;";

        static void Main( string[] args )
        {
            string command = Console.ReadLine();

            if ( command == "readorder" )
            {
                List<Order> orders = ReadOrders();
                foreach ( Order order in orders)
                {
                    Console.WriteLine( "\"" + order.ProductName + "\"" + " за " + order.Price );
                }                
            }
            else if (command == "insert")
            {
                int createdOrderId = InsertOrder(1, "Киви", 30);
                Console.WriteLine("Created post: " + createdOrderId);
            }
            else if (command == "update")
            {
                UpdateOrder(1, "Клубника");
            }
            else if (command == "statistics")
            {
                List<Statistics> statistics = GetStatistics();
                foreach ( Statistics statisticsItem in statistics)
                {
                    Console.WriteLine( "Имя: " + statisticsItem.Name );
                    Console.WriteLine( "Кол-во товаров: " + statisticsItem.OrderCount );
                    Console.WriteLine( "Сумма: " + statisticsItem.OrderSum );
                    Console.WriteLine();
                }
            }
        }

        private static List<Order> ReadOrders()
        {
            List<Order> orders = new List<Order>();
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand command = new SqlCommand() )
                {
                    command.Connection = connection;
                    command.CommandText =
                        @"SELECT
                            [OrderId],
                            [ProductName],
                            [Price],
                            [CustomerId]
                        FROM [Order]";

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        while ( reader.Read() )
                        {
                            var order = new Order
                            {
                                OrderId = Convert.ToInt32( reader[ "OrderId" ] ),
                                ProductName = Convert.ToString( reader[ "ProductName" ] ),
                                Price = Convert.ToInt32( reader[ "Price" ] ),
                                CustomerId = Convert.ToInt32( reader[ "CustomerId" ] ),
                            };
                            orders.Add( order );
                        }
                    }
                }
            }
            return orders;
        }

        private static int InsertOrder(int customerId, string productName, int price)
        {
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = @"
                    INSERT INTO [Order]
                       ([ProductName],
                        [Price],
                        [CustomerId])
                    VALUES 
                       (@productName,
                        @price,
                        @customerId)
                    SELECT SCOPE_IDENTITY()";

                    cmd.Parameters.Add("@productName", SqlDbType.NVarChar).Value = productName;
                    cmd.Parameters.Add("@price", SqlDbType.Int).Value = price;
                    cmd.Parameters.Add("@customerId", SqlDbType.Int).Value = customerId;

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static void UpdateOrder(int orderId, string productName)
        {
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand command = connection.CreateCommand() )
                {
                    command.CommandText = @"
                        UPDATE [Order]
                        SET [ProductName] = @productName
                        WHERE [OrderId] = @orderId";

                    command.Parameters.Add("@orderId", SqlDbType.BigInt).Value = orderId;
                    command.Parameters.Add("@productName", SqlDbType.NVarChar).Value = productName;

                    command.ExecuteNonQuery();
                }
            }
        }

        private static List<Statistics> GetStatistics()
        {
            List<Statistics> statistics = new List<Statistics>();
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand command = connection.CreateCommand() )
                {
                    command.CommandText = @"SELECT [Customer].[Name] as Name, 
                                            COUNT([Order].[CustomerId]) as Count, 
                                            SUM([Order].[Price]) as Sum
                        FROM [Customer]
                        LEFT JOIN [Order] ON [Customer].[CustomerId] = [Order].[CustomerId]
                        GROUP BY ([Customer].[Name])";

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        while ( reader.Read() )
                        {
                            var statisticsItem = new Statistics
                            {
                                Name = Convert.ToString(reader["Name"]),
                                OrderCount = Convert.ToInt32(reader["Count"]),
                                OrderSum = Convert.ToInt32(reader["Sum"])
                            };
                            statistics.Add( statisticsItem );
                        }
                    }
                }    
            };

            return statistics;
        }
    }
}
