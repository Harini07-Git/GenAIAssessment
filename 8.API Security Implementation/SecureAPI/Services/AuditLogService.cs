using System.Text.Json;

namespace SecureAPI.Services
{
    public interface IAuditLogService
    {
        Task LogSuccessfulLoginAsync(string username);
        Task LogFailedLoginAttemptAsync(string username, string reason);
        Task LogTokenRefreshAsync(string username);
        Task LogLogoutAsync(string username);
        Task LogUnauthorizedAccessAsync(string username, string resource);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ILogger<AuditLogService> _logger;
        private readonly string _logPath;
        private const int LogRetentionDays = 90;

        public AuditLogService(ILogger<AuditLogService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _logPath = configuration["AuditLog:Path"] ?? "C:\\Logs\\Security";
            EnsureLogDirectory();
            CleanupOldLogs();
        }

        public async Task LogSuccessfulLoginAsync(string username)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = "Successful Login",
                Username = username,
                IpAddress = GetCurrentIpAddress(),
                Details = "User successfully authenticated"
            };

            await LogEventAsync(logEntry);
        }

        public async Task LogFailedLoginAttemptAsync(string username, string reason)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = "Failed Login",
                Username = username,
                IpAddress = GetCurrentIpAddress(),
                Details = $"Authentication failed: {reason}"
            };

            await LogEventAsync(logEntry);
        }

        public async Task LogTokenRefreshAsync(string username)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = "Token Refresh",
                Username = username,
                IpAddress = GetCurrentIpAddress(),
                Details = "Token refreshed successfully"
            };

            await LogEventAsync(logEntry);
        }

        public async Task LogLogoutAsync(string username)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = "Logout",
                Username = username,
                IpAddress = GetCurrentIpAddress(),
                Details = "User logged out"
            };

            await LogEventAsync(logEntry);
        }

        public async Task LogUnauthorizedAccessAsync(string username, string resource)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = "Unauthorized Access",
                Username = username,
                IpAddress = GetCurrentIpAddress(),
                Details = $"Attempted unauthorized access to: {resource}"
            };

            await LogEventAsync(logEntry);
        }

        private async Task LogEventAsync(AuditLogEntry logEntry)
        {
            try
            {
                var logFile = Path.Combine(_logPath, $"security_log_{DateTime.UtcNow:yyyy-MM-dd}.json");
                var logLine = JsonSerializer.Serialize(logEntry);

                await File.AppendAllLinesAsync(logFile, new[] { logLine });
                _logger.LogInformation("Security event logged: {EventType} for user {Username}", 
                    logEntry.EventType, logEntry.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write security log entry");
                throw;
            }
        }

        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        private void CleanupOldLogs()
        {
            try
            {
                var files = Directory.GetFiles(_logPath, "security_log_*.json");
                var cutoffDate = DateTime.UtcNow.AddDays(-LogRetentionDays);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTimeUtc < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted old log file: {FileName}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old log files");
            }
        }

        private string GetCurrentIpAddress()
        {
            // In a real application, get this from the HttpContext
            return "127.0.0.1";
        }
    }

    public class AuditLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
