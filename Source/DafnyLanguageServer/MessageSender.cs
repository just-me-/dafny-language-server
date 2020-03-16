using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer
{
    /*
     * This class provides for each client supported message an own function for an easy usage. 
     */
    class MessageSender
    {
        private readonly ILanguageServer _router; 

        public MessageSender(ILanguageServer router)
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

        public void SendErrornumber(int countedErrors)
        {
            SendMessage("updateStatusbar", countedErrors.ToString());
        }

        public void SendCurrentDocumentInProcess(string filepath)
        {
            SendMessage("activeVerifiyingDocument", filepath);
        }
    }
}
