using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Tools
{
    /// <summary>
    /// This service provides for each client supported message an own function for an easy usage.
    /// This class can be used like a factory for multiple components. 
    /// </summary>
    class MessageSenderService
    {
        private readonly ILanguageServer _router; 

        public MessageSenderService(ILanguageServer router)
        {
            this._router = router;
        }

        private void SendMessage(string type, string msg)
        {
            _router.Window.SendNotification(type, msg);
        }

        /* Pop-up messages */ 
        public void SendError(string msg)
        {
            SendMessage("ERROR", msg); 
        }

        public void SendWarning(string msg)
        {
            SendMessage("WARNING", msg);
        }

        public void SendInformation(string msg)
        {
            SendMessage("INFO", msg);
        }

        /* Status bar information */
        public void SendServerStarted(string dafnyVersion)
        {
            SendMessage("serverStarted", dafnyVersion);
        }

        public void SendCountedErrors(int countedErrors)
        {
            SendMessage("updateStatusbar", countedErrors.ToString());
        }

        public void SendCurrentDocumentInProcess(string filepath)
        {
            SendMessage("activeVerifiyingDocument", filepath);
        }
    }
}
