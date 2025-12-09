using System;
using System.IO;
using System.Text;

namespace InventarioSistem.Core.Logging
{
    /// <summary>
    /// Logger central do InventarioSistem.
    /// - Registra em arquivo (por dia)
    /// - Notifica ouvintes via evento MessageLogged
    /// </summary>
    public static class InventoryLogger
    {
        private static readonly object _sync = new();

        /// <summary>
        /// Evento disparado sempre que uma mensagem é registrada.
        /// Usado pelo WinForms para exibir o log em tempo real.
        /// </summary>
        public static event Action<string>? MessageLogged;

        private static string LogDirectory
        {
            get
            {
                var appData = Environment.GetEnvironmentVariable("APPDATA")
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "InventorySystem", "logs");
                return dir;
            }
        }

        private static string GetLogFilePath()
        {
            var fileName = $"inventario-{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(LogDirectory, fileName);
        }

        public static void Info(string source, string message)
        {
            Write("INFO", source, message);
        }

        public static void Warn(string source, string message)
        {
            Write("WARN", source, message);
        }

        public static void Error(string source, string message, Exception? ex = null)
        {
            var full = ex == null
                ? message
                : $"{message} :: {ex.GetType().Name}: {ex.Message}";
            Write("ERROR", source, full);
        }

        private static void Write(string level, string source, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var line = $"[{timestamp}] [{level}] [{source}] {message}";

            lock (_sync)
            {
                try
                {
                    Directory.CreateDirectory(LogDirectory);
                    var path = GetLogFilePath();
                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Não deixar o app quebrar se não conseguir logar em disco
                }
            }

            try
            {
                MessageLogged?.Invoke(line);
            }
            catch
            {
                // Ignorar erros em handlers de UI
            }
        }
    }
}
