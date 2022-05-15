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
using Microsoft.AspNetCore.Http;

namespace Sync365
{
    public class Functionso
    {
        public Functionso()
        {
            Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
        }

        //public TDMSApplication ThisApplication;
        public ILogger Logger { get; set; }
        //public TDMSObject thisobject;
        public JsonObject jsonobject;
        public string response;

        public static string JSONReader(HttpRequest Req)
        {
            string RestBody = "";
            var syncIOFeature = Req.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
            if (syncIOFeature != null) syncIOFeature.AllowSynchronousIO = true;
            using (StreamReader reader = new StreamReader(Req.Body, Encoding.UTF8, true, 1024, true))
            {
                RestBody = reader.ReadToEnd();
            };
            return RestBody;
        }

        /* SEND TDMS MESSAGE */
        public static void SendTDMSMessage(TDMSApplication ThisApplication, String mSubject, String mBody, TDMSUser mTo)
        {
            TDMSMessage Msg = ThisApplication.CreateMessage();
            Msg.Subject = mSubject;
            Msg.Body = mBody;
            Msg.ToAdd(mTo);
            Msg.System = false;
            Msg.Send();
        }

        /* SEND POST REQUEST WITH JSON */
        public static string SendRequestPOST (TDMSApplication ThisApplication, string json, string host, string method)
        {
            string Login = ThisApplication.Attributes["A_Str_Login365"].Value.ToString();
            string Password = ThisApplication.Attributes["A_Str_Password365"].Value.ToString();
            
            string url = host + method;
            Console.WriteLine(url);
            string jsonAuth = AuthorizationServiceTDMS.SendTokenRequest(ThisApplication, host, Login, Password);
            if (jsonAuth.Contains("Ошибка")) return jsonAuth;
            string Token = AuthorizationServiceTDMS.GetToken(jsonAuth);
            var request = WebRequest.Create(url);
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", string.Format("Bearer {0}", Token));
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
