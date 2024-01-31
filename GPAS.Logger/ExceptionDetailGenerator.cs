using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Logger
{
    public class ExceptionDetailGenerator
    {
        public string GetDetails(Exception ex)
        {
            if (ex == null)
                return "(null)";
            else
                return
                    "Exception Type: " + ex.GetType().ToString()
                    + Environment.NewLine + "Source: " + ex.Source
                    + Environment.NewLine + "Message: " + ex.Message
                    + Environment.NewLine + "Stack Trace: " + ex.StackTrace
                    + (ex is FaultException<ExceptionDetail>
                        ? Environment.NewLine + "SOAP Fault Exception Detail: " + Environment.NewLine + "["
                            + GetDetails((ex as FaultException<ExceptionDetail>).Detail) + Environment.NewLine + "]"
                        : "")
                    + Environment.NewLine + "Inner Exception:" +
                        (ex.InnerException == null ? " " : Environment.NewLine + Environment.NewLine)
                        + GetDetails(ex.InnerException);
        }

        private string GetDetails(ExceptionDetail exDetail)
        {
            if (exDetail == null)
                return ("(null)");
            else
                return
                    "Message: " + exDetail.Message
                    + Environment.NewLine + "Stack Trace: " + exDetail.StackTrace
                    + Environment.NewLine + "Type: " + exDetail.Type
                    + Environment.NewLine + "Inner Exception: " + Environment.NewLine + "["
                        + GetDetails(exDetail.InnerException)
                        + Environment.NewLine + "]";
        }
        
        public Exception AppendWebExceptionResonse(WebException ex)
        {
            string responseMessage = string.Empty;
            try
            {
                Stream response = ex.Response.GetResponseStream();
                StreamUtility utility = new StreamUtility();
                responseMessage = utility.GetStringFromStream(utility.ConvertStreamToMemoryStream(response));
            }
            catch
            { }
            if (responseMessage.Equals(string.Empty))
            {
                return ex;
            }
            else
            {
                return new Exception(responseMessage, ex);
            }
        }
    }
}
