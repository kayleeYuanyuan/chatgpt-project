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

public partial class OnRequestChatquestion : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        /* Record Start timestamp */
        DateTime OnSet = DateTime.Now;
        StringBuilder response = new StringBuilder();
        string question = Request["question"];
        string fileID = Request["fileID"];
        string assistantID = Request["assistantID"];
        string threadID = Request["threadID"];

        /* Setup response */
        IResponse iResponse = new IResponse(false, StringBuffer.AuthenticationFail);
        LogItem log = new LogItem("", false, "", this.GetType().Name, Config.Domain);

        string result = "";

        try {
            // no question
            if (string.IsNullOrEmpty(question)) 
            {
                result = "Type your question first! ";
                log.SetLog(true, StringBuffer.ApiError, result);               
            }
            else if (string.IsNullOrEmpty(fileID) || string.IsNullOrEmpty(assistantID) || string.IsNullOrEmpty(threadID))
            {
                result = "Lack of information! ";
                log.SetLog(true, StringBuffer.ApiError, result);
            }
            // answer question
            else if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(fileID) && !string.IsNullOrEmpty(assistantID) && !string.IsNullOrEmpty(threadID)) 
            {
                string messageID = ChatController.CreateMessage(threadID, fileID, question);

                string runs = ChatController.CreateRuns(threadID, assistantID);
                string text = ChatController.ListMessage(threadID);
                List<string> answer = ChatController.extractAnswer(text);
                result = string.Join("!!", answer);


                log.SetLog(true, StringBuffer.ApiComplete, "question complete", result);
            }
            else
            {
                result = "Wrong! ";
                log.SetLog(true, StringBuffer.ApiComplete, result);
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
