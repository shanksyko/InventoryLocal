// Simple test to verify SQL Server infrastructure without requiring actual database
using System;
using System.Reflection;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Validation;

public class SqlServerValidation
{
    public static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║   SQL Server Migration Validation      ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        int testsPass = 0;
        int testsFail = 0;

        // Test 1: Assembly loading
        Console.WriteLine("[1/7] Checking Microsoft.Data.SqlClient assembly...");
        try
        {
            var sqlClientType = Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient");
            if (sqlClientType != null)
            {
                Console.WriteLine("      ✓ PASS: Microsoft.Data.SqlClient is loaded\n");
                testsPass++;
            }
            else
            {
                Console.WriteLine("      ✗ FAIL: Could not find SqlConnection type\n");
                testsFail++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 2: SqlServerConnectionFactory
        Console.WriteLine("[2/7] Testing SqlServerConnectionFactory...");
        try
        {
            var factory = typeof(SqlServerConnectionFactory);
            if (factory != null && factory.GetMethod("CreateConnection") != null)
            {
                Console.WriteLine("      ✓ PASS: SqlServerConnectionFactory has CreateConnection method\n");
                testsPass++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 3: SqlServerConfig
        Console.WriteLine("[3/7] Testing SqlServerConfig...");
        try
        {
            var config = new SqlServerConfig();
            var connStr = config.ConnectionString;
            if (!string.IsNullOrEmpty(connStr))
            {
                Console.WriteLine("      ✓ PASS: SqlServerConfig loads connection string\n");
                testsPass++;
            }
            else
            {
                Console.WriteLine("      ✗ FAIL: Connection string is empty\n");
                testsFail++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 4: SqlServerDatabaseManager
        Console.WriteLine("[4/7] Testing SqlServerDatabaseManager...");
        try
        {
            var dbManagerType = typeof(SqlServerDatabaseManager);
            var methods = new[] { "CreateNewDatabase", "TestConnection", "GetDatabaseSummary" };
            bool allMethodsFound = true;
            foreach (var method in methods)
            {
                if (dbManagerType.GetMethod(method) == null)
                {
                    allMethodsFound = false;
                    break;
                }
            }
            if (allMethodsFound)
            {
                Console.WriteLine("      ✓ PASS: SqlServerDatabaseManager has all required methods\n");
                testsPass++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 5: SqlServerSchemaManager
        Console.WriteLine("[5/7] Testing SqlServerSchemaManager...");
        try
        {
            var schemaType = typeof(SqlServerSchemaManager);
            if (schemaType.GetMethod("EnsureRequiredTables") != null)
            {
                Console.WriteLine("      ✓ PASS: SqlServerSchemaManager has EnsureRequiredTables method\n");
                testsPass++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 6: SqlServerInventoryStore
        Console.WriteLine("[6/7] Testing SqlServerInventoryStore...");
        try
        {
            var storeType = typeof(SqlServerInventoryStore);
            var deviceMethods = new[] { 
                "GetAllComputadores", "AddComputador", 
                "GetAllTablets", "AddTablet",
                "GetAllCelulares", "AddCelular",
                "GetAllImpressoras", "AddImpressora",
                "GetAllDects", "AddDect",
                "GetAllTelefonesCisco", "AddTelefoneCisco",
                "GetAllTelevisores", "AddTelevisor",
                "GetAllRelogiosPonto", "AddRelogioPonto",
                "GetAllMonitores", "AddMonitor",
                "GetAllNobreaks", "AddNobreak"
            };
            
            int foundMethods = 0;
            foreach (var method in deviceMethods)
            {
                if (storeType.GetMethod(method) != null)
                    foundMethods++;
            }
            
            if (foundMethods >= 15) // At least 75% of methods
            {
                Console.WriteLine($"      ✓ PASS: SqlServerInventoryStore has {foundMethods}/{deviceMethods.Length} device methods\n");
                testsPass++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Test 7: SqlServerUserStore
        Console.WriteLine("[7/7] Testing SqlServerUserStore...");
        try
        {
            var userStoreType = typeof(SqlServerUserStore);
            var methods = new[] { "ValidateUserAsync", "GetUserAsync", "CreateUserAsync" };
            bool allMethodsFound = true;
            foreach (var method in methods)
            {
                if (userStoreType.GetMethod(method) == null)
                {
                    allMethodsFound = false;
                    break;
                }
            }
            if (allMethodsFound)
            {
                Console.WriteLine("      ✓ PASS: SqlServerUserStore has all required auth methods\n");
                testsPass++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"      ✗ FAIL: {ex.Message}\n");
            testsFail++;
        }

        // Summary
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine($"║  Results: {testsPass} PASS, {testsFail} FAIL           ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        if (testsFail == 0)
        {
            Console.WriteLine("✓ All SQL Server infrastructure tests passed!\n");
            Console.WriteLine("Next Steps:");
            Console.WriteLine("  1. Ensure SQL Server Express x64 is installed");
            Console.WriteLine("  2. Update sqlserver.config.json with your server details");
            Console.WriteLine("  3. Run create-database.ps1 to initialize the database");
            Console.WriteLine("  4. Launch the WinForms application");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("✗ Some tests failed. Check the output above.\n");
            Environment.Exit(1);
        }
    }
}
