namespace CVSante.Services
{
    public interface IHistoryService
    {
        Task LogActionAsync(int? userId, int paramId, string action, string additionalInfo = "");
    }
}
