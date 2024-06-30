using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

public partial class OnRequestChatfileUpload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        /* Record Start timestamp */
        DateTime OnSet = DateTime.Now;
        StringBuilder response = new StringBuilder();
        string fileName = Request["fileName"];

        /* Setup response */
        IResponse iResponse = new IResponse(false, StringBuffer.AuthenticationFail);
        LogItem log = new LogItem("", false, "", this.GetType().Name, Config.Domain);

        string result = "";

        try {
            // upload file 
            if (string.IsNullOrEmpty(fileName))
            {
                result = "Upload Fail, no file! ";
                log.SetLog(true, StringBuffer.ApiError, result);
            }
            else
            {
                HttpContext context = HttpContext.Current;
                HttpPostedFile file = context.Request.Files[0];
                string filePath = ChatController.getPath(file, context);
                string fileID = ChatController.UploadFile(filePath);
                string assistantID = ChatController.CreateAssistant(fileID);
                string threadID = ChatController.CreateThread();
                result = fileID + " " + assistantID + " " + threadID;
                log.SetLog(true, StringBuffer.ApiComplete, "upload complete", filePath);

            }
        }
        catch (Exception ex)
        {
            result = ex.StackTrace;
            log.SetLog(false, StringBuffer.ApiError, "Exception Catached", ex.Message);
        }

        string dataPackage = iResponse.ToJson();
        iResponse.complete = Utility.GetMsDiffFromNow(OnSet);
        log.packageSize = dataPackage.Length;
        log.complete = iResponse.complete;
        //response.Append(iResponse.ToJson());
        Response.Write(result);
        Logger.Write(log);
        return;
    }

}
