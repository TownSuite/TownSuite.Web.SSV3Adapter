namespace TownSuite.Web.SSV3Facade.Interfaces
{
    public interface ISSV3Promethues : IDisposable
    {
        void Dispose();
        void EndRequest(string code, string method, string controller, string action);
        void ExceptionTriggered();
    }
}