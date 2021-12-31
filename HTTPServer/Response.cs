using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
            this.code = code;
            // TODO: Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])
            headerLines.Add(contentType);
            headerLines.Add(content.Length.ToString());
            headerLines.Add(DateTime.Now.ToString());
            string status = GetStatusLine(code);

            // TODO: Create the request string
            if (code.Equals(StatusCode.Redirect))
            {
                headerLines.Add(redirectoinPath);
                responseString = status + "\r\n" + "content-Type: " + headerLines[0] + "\r\n" + "Content-Length: " + headerLines[1] + "\r\n"
                    + "Date: " + headerLines[2] + "\r\n" + "Location: " + headerLines[3] + "\r\n" + "\r\n" + content;
            }

            else
            {
                responseString = status + "\r\n" + "content-Type: " + headerLines[0] + "\r\n" + "Content-Length: " + headerLines[1] + "\r\n"
                    + "Date: " + headerLines[2] + "\r\n" + "\r\n" + content;
            }

        }

        private string GetHeaderLines(List<string> headerlineslist)
        {
            string headerlines = string.Empty;
            foreach (string value in headerlineslist)
            {
                headerlines += value + "\r\n";
            }
            return headerlines;
        }

        private string GetStatusLine(StatusCode code)
        {
            // TODO: Create the response status line and return it
            string statusLine = string.Empty;
            if (code.Equals(StatusCode.OK))
            {
                statusLine = Configuration.ServerHTTPVersion + " " + (int)StatusCode.OK + " " + StatusCode.OK.ToString();
            }

            else if (code.Equals(StatusCode.Redirect))
            {
                statusLine = Configuration.ServerHTTPVersion + " " + (int)StatusCode.Redirect + " " + StatusCode.Redirect.ToString();
            }

            else if (code.Equals(StatusCode.BadRequest))
            {
                statusLine = Configuration.ServerHTTPVersion + " " + (int)StatusCode.BadRequest + " " + StatusCode.BadRequest.ToString();
            }

            else if (code.Equals(StatusCode.InternalServerError))
            {
                statusLine = Configuration.ServerHTTPVersion + " " + (int)StatusCode.InternalServerError + " " + StatusCode.InternalServerError.ToString();
            }

            else if (code.Equals(StatusCode.NotFound))
            {
                statusLine = Configuration.ServerHTTPVersion + " " + (int)StatusCode.NotFound + " " + StatusCode.NotFound.ToString();
            }

            return statusLine;
        }
    }
}
