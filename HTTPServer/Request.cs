using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLines;
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        string requestString;
        string[] contentLines;
        string[] reqList;

        public Request(string requestString)
        {
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {

            String[] separators = new String[] { "\r\n" };

            //TODO: parse the receivedRequest using the \r\n delimeter

            requestLines = requestString.Split(separators, 10, StringSplitOptions.None);

            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)

            // Parse Request line

            if (!ParseRequestLine())
            {
                return false;
            }

            if (!ValidateIsURI(relativeURI))
            {

                return false;
            }

            // Validate blank line exists
            if (!ValidateBlankLine())
            {
                return false;
            }

            // Load header lines into HeaderLines dictionary

            if (!LoadHeaderLines())
            {
                return false;
            }
            return true;
        }

        private bool ParseRequestLine()
        {
            //throw new NotImplementedException();
            reqList = requestLines[0].Split(' ');

            String Req_method = reqList[0];

            relativeURI = reqList[1];

            String version = reqList[2];

            if (requestLines.Length < 3)
            {
                return false;
            }

            switch (version)
            {
                case "HTTP/0.9":

                    httpVersion = HTTPVersion.HTTP09;

                    break;
                case "HTTP/1.1":

                    httpVersion = HTTPVersion.HTTP11;

                    break;
                case "HTTP/1.0":

                    httpVersion = HTTPVersion.HTTP10;
                    break;
                default:
                    httpVersion = HTTPVersion.HTTP09;
                    break;
            }

            // Choosing method type
            switch (Req_method)
            {
                case "GET":
                    method = RequestMethod.GET;
                    break;

                case "POST":
                    method = RequestMethod.POST;
                    break;

                case "HEAD":
                    method = RequestMethod.HEAD;
                    break;
                default:
                    Console.WriteLine("Method Error!!");
                    break;
            }
            return true;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {
            //throw new NotImplementedException();

            String[] headerLinesArr = new String[100];

            for (int i = 1; i < requestLines.Length; i++)
            {
                if (requestLines[i] == "\r\n\r\n")
                {
                    break;
                }
                else
                {
                    headerLinesArr[i - 1] = requestLines[i];
                }
            }


            String[] temp = new String[2];
            String[] separator = new String[] { ": " };
            for (int i = 0; i < headerLinesArr.Length; i++)
            {
                temp = headerLinesArr[i].Split(separator, 10, StringSplitOptions.None);
                headerLines.Add(temp[0], temp[1]);
            }

            if (headerLines.Count != headerLinesArr.Length)
            {
                return false;
            }
            return true;
        }

        private bool ValidateBlankLine()
        {

            if (!requestString.Contains("\r\n\r\n"))
            {
                return false;
            }
            //throw new NotImplementedException();
            return true;
        }

    }
}
