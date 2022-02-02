using Microsoft.AspNetCore.Mvc;
using Tdms.Api;
using Tdms.Log;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Timers;
using System.Net.Http;

namespace Sync365
{
    public class Functions
    {
        public TDMSApplication ThisApplication;
        public ILogger Logger { get; set; }
        public TDMSObject thisobject;
        public JsonObject jsonobject;
        public string response;
        /* SEND TDMS MESSAGE */
        public void SendTDMSMessage(String mSubject, String mBody, TDMSUser mTo)
        {
            TDMSMessage Msg = ThisApplication.CreateMessage();
            Msg.Subject = mSubject;
            Msg.Body = mBody;
            Msg.ToAdd(mTo);
            Msg.System = false;
            Msg.Send();
        }

        /* SEND POST REQUEST WITH JSON */
        public string SendRequestPOST(string json, string url)
        {
            //var url = ThisApplication.Attributes["a_url_365"].Value + "/api/GPPimportRZstatus";
            var request = WebRequest.Create(url);
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            using var response = request.GetResponse();
            using var respStream = response.GetResponseStream();
            using var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
            return data;
        }
    }
}
