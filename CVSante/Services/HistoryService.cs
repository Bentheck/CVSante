using CVSante.Data;
using CVSante.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CVSante.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly CvsanteContext _context;

        public HistoryService(CvsanteContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(int? userId, int paramId, string action, string additionalInfo = "")
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action cannot be null or empty.", nameof(action));
            }

            // Prepare action string with additional info
            string formattedAction = !string.IsNullOrWhiteSpace(additionalInfo)
                ? $"{action} - {additionalInfo}"
                : action;

            // Check if the user exists if userId is provided
            if (userId.HasValue)
            {
                var userExists = await _context.UserInfos
                    .AnyAsync(ui => ui.FkUserId == userId.Value);

                if (!userExists)
                {
                    throw new InvalidOperationException($"User with ID {userId.Value} does not exist.");
                }
            }

            // Create and add HistoriqueParam
            var history = new HistoriqueParam
            {
                FkUserId = userId,  // Set to null if userId is not provided
                FkParamId = paramId,
                Action = formattedAction,
                Date = DateTime.UtcNow
            };

            _context.HistoriqueParams.Add(history);
            _context.SaveChanges();  // Use SaveChangesAsync
        }
    }
}