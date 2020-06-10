using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Tools
{
    /// <summary>
    /// This service provides methods to send messages to the client.
    /// Each severity level supported by the client is represented with an own method for easy usage.
    /// </summary>
    public class MessageSenderService
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

        public void SendServerStarted(string dafnyVersion, string additionalStatusbarInformation = "")
        {
            SendMessage("serverStarted", dafnyVersion + additionalStatusbarInformation);
            /* Server version for updating */
            SendMessage("dafnyLanguageServerVersionReceived", dafnyVersion);
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