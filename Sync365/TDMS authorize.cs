using System;
using System.Net;
using System.IO;
using System.Text;
using Tdms.Api;

/// <summary>
/// Класс авторизации в TDMS Server по REST API
/// </summary>
public class AuthorizationServiceTDMS
{
    public AuthorizationServiceTDMS()
    {
    }

    /// <summary>
    /// Функция отправляет запрос на получение токена авторизации у TDMS Server
    /// </summary>
    /// <param name="ServerHost">Адрес TDMS Server, например http:\\localhost:444\""</param>
    /// <param name="Login">Логин аккаунта</param>
    /// <param name="Password">Пароль аккаунта</param>
    /// <returns></returns>
    public static string SendTokenRequest(TDMSApplication ThisApplication, string ServerHost, string Login, string Password)
    {
        string jsonString = "";
        if (ServerHost == "" || ServerHost == null || Login == "" || Login == null) return jsonString;

        try
        {
            Password = Password ?? "";
            string Auth = Convert.ToBase64String(Encoding.Default.GetBytes(Login + ":" + Password));

            if (ServerHost.Substring(ServerHost.Length - 2, 1) != "/") ServerHost = ServerHost + "/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerHost + "token");
            request.Method = "POST";
            request.Accept = "/";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", string.Format("Basic {0}", Auth));
            using (Stream requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.WriteLine("grant_type=client_credentials");
            }
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            jsonString = sr.ReadToEnd();
            sr.Close();
        }
        catch (WebException ex)
        {
            WebResponse response = (ex.Response as WebResponse);
            if (response == null)
                throw;
            StreamReader sr = new StreamReader(response.GetResponseStream());
            jsonString = sr.ReadToEnd();
            Newtonsoft.Json.Linq.JObject jObj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
            if (jObj != null)
            {
                Newtonsoft.Json.Linq.JToken Err0;
                Newtonsoft.Json.Linq.JToken Err1;
                jObj.TryGetValue("error", out Err0);
                jObj.TryGetValue("error_description", out Err1);
                if (Err0 != null)
                    jsonString = "Ошибка: " + Err0.ToString() + "\n";
                if (Err1 != null)
                    jsonString = jsonString + "Описание: " + Err1.ToString() + "\n";
            }
            else
            {
                if (ex != null)
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }
        return jsonString;
    }

    /// <summary>
    /// Функция вовзращает токен из запроса Json
    /// </summary>
    /// <param name="JsonToken">Тело Json в строковом виде</param>
    /// <returns></returns>
    public static string GetToken(string JsonToken)
    {
        string Token = "";
        if (JsonToken == "" || JsonToken == null) return Token;
        Newtonsoft.Json.Linq.JObject jObj = Newtonsoft.Json.Linq.JObject.Parse(JsonToken);
        if (jObj != null)
            Token = jObj.Value<string>("access_token");
        return Token;
    }

    /// <summary>
    /// Процедура добавляет в запрос HttpWebRequest заголовок с авторизацией по токену
    /// </summary>
    /// <param name="request">Ссылка на запрос</param>
    /// <param name="Token">Токен</param>
    public static bool SetAuthorizationHeaders(HttpWebRequest request, string Token)
    {
        bool Ret = false;
        if (Token == "" || Token == null || request == null) return Ret;
        request.Headers.Add("Authorization", string.Format("Bearer {0}", Token));
        return true;
    }

    /// <summary>
    /// Процедура добавляет в запрос WebRequest заголовок с авторизацией по токену
    /// </summary>
    /// <param name="request">Ссылка на запрос</param>
    /// <param name="Token">Токен</param>
    public static void SetAuthorizationHeaders(WebRequest request, string Token)
    {
        request.Headers.Add("Authorization", string.Format("Bearer {0}", Token));
    }
}

//Пример использования
/*
//Авторизация в TDMS
string JsonToken = AuthorizationServiceTDMS.SendTokenRequest(ThisApplication, sHost, "rest", "tdm365");
if (JsonToken.Contains("Ошибка")) return JsonToken; //Если ошибка авторизации, то возвращаем ее
string Token = AuthorizationServiceTDMS.GetToken(JsonToken);

//Создание запроса
WebRequest request = WebRequest.Create("");
//Авторизация в TDMS
AuthorizationServiceTDMS.SetAuthorizationHeaders(request, Token);
*/
