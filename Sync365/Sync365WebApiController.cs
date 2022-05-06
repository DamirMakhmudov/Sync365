﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Sync365
{
    /*
    public class Functions
    {
        public TDMSApplication ThisApplication;
        public ILogger Logger { get; set; }
        public TDMSObject thisobject;
        public JsonObject jsonobject;
        public string response;

        public Functions(TDMSApplication application)
        {
            ThisApplication = application;
            Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
        }

    public string SendRequestPOST(string json, string url)
    {
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
    */
    /* SCHEDULER 2.1 */
    [TdmsApi("ShTask")]
    public class ShTask
    {
        TDMSApplication ThisApplication;
        public ILogger Logger { get; set; }
        public string response;
        public string systemname = "ИУС СЭТД";

        public ShTask(TDMSApplication application)
        {
            ThisApplication = application;
            Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
        }
        public void Execute()
        {
            //var Functions = new Functions(ThisApplication);
            try
            {
                //Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
                Logger.Info("Flow 2.1 started");
                TDMSQuery qO_ClaimRegistry = ThisApplication.CreateQuery();
                qO_ClaimRegistry.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, "O_ClaimRegistry");
                qO_ClaimRegistry.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "= Null or =''", "A_Bool_Started");

                Logger.Info("ClaimRegistry count: " + qO_ClaimRegistry.Objects.Count.ToString());

                foreach (TDMSObject O_ClaimRegistry in qO_ClaimRegistry.Objects)
                {
                    Logger.Info(O_ClaimRegistry.Description);
                    O_ClaimRegistry.Attributes["A_Bool_Started"].Value = true;
                }

                foreach (TDMSObject O_ClaimRegistry in qO_ClaimRegistry.Objects)
                {
                    try
                    {
                        TDMSQuery qO_DocClaim = ThisApplication.CreateQuery();
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, "O_DocClaim");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, O_ClaimRegistry, "A_Ref_Parent");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "= Null or =''", "A_Bool_Started");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "<> ''", "A_Str_Files");

                        foreach (TDMSObject O_DocClaim in qO_DocClaim.Objects)
                        {
                            O_DocClaim.Attributes["A_Bool_Started"].Value = true;

                            String StrFiles = O_DocClaim.Attributes["A_Str_Files"].Value.ToString();
                            string[] words = StrFiles.Split(';');
                            foreach (var word in words)
                            {
                                if (word != "")
                                {
                                    TDMSFile newFile = O_DocClaim.Files.Create("FILE_ALL", word);
                                }
                            }
                        }

                        ResponseJson rjsonobject = new ResponseJson();
                        rjsonobject.SystemName = systemname;
                        rjsonobject.Result = "true";
                        rjsonobject.Date = DateTime.Now.ToString();
                        rjsonobject.Completed = true;
                        rjsonobject.Objects = new List<jObject>();

                        jObject rObject = new jObject();
                        rObject.ObjGuidExternal = O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value.ToString();
                        rObject.ObjGuid = O_ClaimRegistry.GUID;
                        rjsonobject.Objects.Add(rObject);

                        var json = System.Text.Json.JsonSerializer.Serialize(rjsonobject);
                        //var data = Functions.SendRequestPOST(json, ThisApplication.Attributes["a_url_365"].Value + "/api/GPPimportRZstatus");
                        var data = Functionso.SendRequestPOST(ThisApplication, json, ThisApplication.Attributes["a_url_365"].Value.ToString(), "/api/GPPimportRZstatus");

                        TDMSObject O_Package_Unload = O_ClaimRegistry.Parent;
                        TDMSObject O_Project = O_Package_Unload.Attributes["A_Ref_Project"].Object;
                        ThisApplication.SaveChanges();

                        TDMSObject O_Doc = O_ClaimRegistry.Attributes["A_Ref_Doc"].Object;
                        if (O_Doc != null)
                        {
                            TDMSUser User = O_Doc.Attributes["A_User_Author"].User;
                            if (User != null)
                            {
                                Functionso.SendTDMSMessage(ThisApplication, $"Реестр замечаний: \"{O_ClaimRegistry.Description}\"", $"Получен реестр замечаний \"{O_ClaimRegistry.Description}\" по следующему пакету загрузки \"{O_Package_Unload.Description}\"", User);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ResponseJson rjsonobject = new ResponseJson();
                        rjsonobject.SystemName = systemname;
                        rjsonobject.Result = ex.Message + "\n" + ex.StackTrace;
                        rjsonobject.Date = DateTime.Now.ToString();
                        rjsonobject.Completed = false;
                        var json = System.Text.Json.JsonSerializer.Serialize(rjsonobject);
                        //var data = Functions.SendRequestPOST(json, ThisApplication.Attributes["a_url_365"].Value + "/api/GPPimportRZstatus");
                        var data = Functionso.SendRequestPOST(ThisApplication, json, ThisApplication.Attributes["a_url_365"].Value.ToString(), "/api/GPPimportRZstatus");
                        Logger.Error(data);
                    }
                };
                Logger.Info("2.1 finished");
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Error($"2.1 finished with: {response}");
            }
        }
    }

    /* REST */
    public class Sync365WebApiController : ControllerBase
    {
        //[TdmsAuthorize] // for avoid authorization
        public TDMSApplication ThisApplication;
        public ILogger Logger { get; set; }
        public TDMSObject thisobject;
        public JsonObject jsonobject;
        public string response;

        public Sync365WebApiController(TDMSApplication application)
        {
            ThisApplication = application;
            Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
        }

        /* Test */
        //[Authorize] 
        [Route("api/test"), HttpPost]
        public string Test([FromBody] JsonObject jsonobjectO)
        {
            string host = "http://user.com";
            string method = "api/test";
            string res = host + method;
            Logger.Info(res);

            return "true";
            ResponseJson rjsonobject = new ResponseJson();
            rjsonobject.SystemName = "hello";
            rjsonobject.Date = DateTime.Now.ToString();
            rjsonobject.Completed = false;
            var json = System.Text.Json.JsonSerializer.Serialize(rjsonobject);

            var data = Functionso.SendRequestPOST(ThisApplication, json, "http://192.168.16.113:444/", "api/authtest");
            Logger.Info(data);
            return "true";
        }

        /* Authorization test */
        [Authorize]
        [Route("api/authtest"), HttpPost]
        //public string AuthTest([FromBody] ResponseJson responsejsonO)
        public string AuthTest()
        {
            var Req = this.Request;
            ResponseJson responsejsonO = System.Text.Json.JsonSerializer.Deserialize<ResponseJson>(Functionso.JSONReader(Req));
            Logger.Info(responsejsonO.SystemName);
            return "true after auth";
        }

        /* Flow 0.1 PROJECT */
        [Route("api/GPPtransferProjectResponse"), HttpPost]
        public string GPPtransferProjectResponse([FromBody] ResponseJson jsonobjectO)
        {
            //var Functions = new Functions(ThisApplication);
            try
            {
                Logger.Info("0.1 GPPtransferProjectResponse: started");
                String mBody = "";
                TDMSObject project = ThisApplication.GetObjectByGUID(jsonobjectO.Objects[0].ObjGuidExternal);
                Logger.Info(jsonobjectO.Objects[0].ObjGuidExternal);
                Logger.Info(jsonobjectO.Objects.Count.ToString());

                if (jsonobjectO.Completed)
                {
                    if (jsonobjectO.Objects[0].ObjStatus == "STATUS_Prj_Created")
                    {
                        project.Attributes["A_Bool_Published_365"].Value = true;
                        mBody = $"Проект \"{project.Attributes["A_Str_Designation"].Value}\" успешно доставлен";
                    }
                }
                Functionso.SendTDMSMessage(ThisApplication, mBody, mBody, project.Attributes["A_User_GIP"].User);

                ThisApplication.SaveChanges();
                ThisApplication.SaveContextObjects();
                response = "true";
                Logger.Info("GPPtransferProjectResponse: finished");
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Info($"GPPtransferProjectResponse: finished with: {response}");
                return response;
            }
        }

        /* Flow 0.2 PROJECT */
        [Route("api/GPPtransferProjectLaunched"), HttpPost]
        public string GPPtransferProjectLaunched([FromBody] ResponseJson jsonobjectO)
        {
            try
            {
                Logger.Info("0.2 GPPtransferProjectLaunched: started");
                String textmessage = "";
                TDMSObject project = ThisApplication.GetObjectByGUID(jsonobjectO.Objects[0].ObjGuidExternal);
                if (jsonobjectO.Completed)
                {
                    if (jsonobjectO.Objects[0].ObjStatus == "STATUS_Prj_InProgress")
                    {
                        textmessage = $"Проект \"{project.Attributes["A_Str_Designation"].Value}\" успешно запущен";
                    }
                }

                //var Functions = new Functions(ThisApplication);
                Functionso.SendTDMSMessage(ThisApplication, textmessage, textmessage, project.Attributes["A_User_GIP"].User);

                ThisApplication.SaveChanges();
                ThisApplication.SaveContextObjects();
                response = "true";
                Logger.Info("GPPtransferProjectLaunched: finished");
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Info($"GPPtransferProjectLaunched: finished with: {response}");
                return response;
            }
        }

        /* Flow 1.1 PERDOC */
        [Route("api/GPPtransferDocResponse"), HttpPost]
        public string GPPtransferDocResponse([FromBody] ResponseJson jsonobject)
        {
            try
            {
                Logger.Info("GPPtransferDocResponse: started");
                TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
                TDMSAttributes Attrs = O_Package_Unload.Attributes;
                if (jsonobject.Completed)
                {
                    Attrs["A_Bool_Load"].Value = true;
                    Attrs["A_Date_Load"].Value = DateTime.Now;
                    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_OnReview"];
                    Logger.Info("true");
                }
                else
                {
                    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_Cancel"];
                    TDMSMessage Msg = ThisApplication.CreateMessage();
                    Msg.Subject = "Ошибка при импорте пакета \"" + O_Package_Unload.Description + "\"";
                    Msg.Body = jsonobject.Result.ToString();
                    Msg.ToAdd(ThisApplication.CurrentUser);
                    Msg.System = false;
                    Msg.Send();
                    Logger.Info("some error");
                }

                ThisApplication.SaveChanges();
                ThisApplication.SaveContextObjects();
                response = "true";
                Logger.Info("GPPtransferDocResponse: finished");
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Info($"GPPtransferDocResponse: finished with: {response}");
                return response;
            }
        }

        /* Flow 2 old RZ */
        [Route("api/GPPgetClaimRegistryOld"), HttpPost]
        public string GPPgetClaimRegistryOld([FromBody] JsonPackageRZ jsonobject)
        {
            try
            {
                Logger.Info("GPPgetClaimRegistry: started");
                TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());

                TDMSObject O_ClaimRegistry = O_Package_Unload.Objects.Create("O_ClaimRegistry");
                O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value = jsonobject.RZ.Guid;
                O_ClaimRegistry.Attributes["A_Str_Name"].Value = jsonobject.RZ.ATTR_NAME_REGISTRY;
                O_ClaimRegistry.Attributes["A_Date_Create"].Value = DateTime.Parse(jsonobject.RZ.ATTR_REGYSTRY_CREATION_DATE);
                O_ClaimRegistry.Attributes["A_Str_Designation"].Value = jsonobject.RZ.ATTR_Registry_Num;
                O_ClaimRegistry.Attributes["A_Dat_Req_Deadline"].Value = jsonobject.RZ.ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE;
                jUser UserInitiated = jsonobject.RZ.ATTR_Registry_UserInitiated;
                O_ClaimRegistry.Attributes["A_Str_ClaimAuthor"].Value = $"{UserInitiated.LastName} {UserInitiated.FirstName} {UserInitiated.MiddleName}, {UserInitiated.Tel}, {UserInitiated.Mail}";
                TDMSObject O_Document = ThisApplication.GetObjectByGUID(jsonobject.RZ.TD_External_Guid.ToString());
                O_ClaimRegistry.Attributes["A_Ref_Doc"].Value = O_Document;
                TDMSUser mUser = O_Document.Attributes["A_User_Author"].User;
                O_ClaimRegistry.Attributes["A_User_Author"].Value = mUser;
                O_ClaimRegistry.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], mUser);
                TDMSGroup gr_gup = null;

                TDMSUser A_User_Author = O_Document.Attributes["A_User_Author"].User;
                if (A_User_Author != null)
                {
                    TDMSTableAttribute tdept = A_User_Author.Attributes["A_Table_Depts"].Rows;
                    if (tdept.Count > 0)
                    {
                        TDMSObject mdept = null;
                        foreach (TDMSTableAttributeRow row in tdept)
                        {
                            if (row.Attributes["A_Bool_MainDept"].Value.ToString() == "True")
                            {
                                mdept = row.Attributes["A_Ref_Dept"].Object;
                            }
                        }
                        if (mdept != null)
                        {
                            TDMSQuery qDeps = ThisApplication.Queries["Q_Dept_Users"];
                            qDeps.SetParameter("DeptObj", mdept);
                            string scode = mdept.Attributes["A_Str_NumCode"].Value.ToString().Replace(".", "_");

                            if (!ThisApplication.Groups.Has("G_" + scode))
                            {
                                TDMSGroup gr = ThisApplication.Groups.Create();
                                gr.SysName = "G_" + scode;
                                gr.Description = $"{mdept.Attributes["A_Str_NumCode"].Value} { mdept.Attributes["A_Str_Name"].Value}";
                                ThisApplication.SaveChanges();
                            }
                            gr_gup = ThisApplication.Groups["G_" + scode];

                            foreach (TDMSUser user in qDeps.Users)
                            {
                                gr_gup.Users.Add(user);
                            }
                            ThisApplication.SaveChanges();
                        }
                    }
                };
                ThisApplication.SaveContextObjects();

                foreach (jRemark remark in jsonobject.Remarks)
                {
                    String FilesString = "";
                    TDMSObject O_DocClaim = O_ClaimRegistry.Objects.Create("O_DocClaim");
                    O_DocClaim.Attributes["A_Str_Designation"].Value = remark.ATTR_Remark_Num;
                    O_DocClaim.Attributes["A_Str_ClaimDesc"].Value = remark.ATTR_Remark;
                    O_DocClaim.Attributes["A_Str_AnswerDesc"].Value = remark.ATTR_Answer;
                    O_DocClaim.Attributes["A_Str_Answer"].Value = remark.ATTR_Answer_Type;
                    O_DocClaim.Attributes["A_Int_DocVersion"].Value = remark.ATTR_TechDoc_Version;
                    O_DocClaim.Attributes["A_Str_GUID_External"].Value = remark.Guid;

                    jUser authorZM = remark.ATTR_AUTHOR_ZM;
                    O_DocClaim.Attributes["A_Str_ClaimAuthor"].Value = $"{authorZM.LastName} {authorZM.FirstName} {authorZM.MiddleName}, {authorZM.Tel}, {authorZM.Mail}";

                    //jUser authorAnswer = remark.ATTR_AUTHOR_ANSWER;
                    //O_DocClaim.Attributes["A_User_AnswerAuthor"].Value = $"{authorAnswer.LastName} {authorAnswer.FirstName} {authorAnswer.MiddleName}, {authorAnswer.Tel}, {authorAnswer.Mail}";

                    O_DocClaim.Attributes["A_Date_Answer"].Value = remark.ATTR_Answer_Date;
                    O_DocClaim.Attributes["A_Date_Create"].Value = remark.ATTR_Remark_Date;
                    O_DocClaim.Attributes["A_Str_Claim"].Value = remark.ATTR_REMARK_TYPE;
                    O_DocClaim.Attributes["A_Ref_DocClaimRegistry"].Value = O_ClaimRegistry;
                    //O_DocClaim.Attributes["A_User_Author"].Value = mUser;
                    if (gr_gup != null)
                    {
                        O_DocClaim.Roles.Create(ThisApplication.RoleDefs["R_Dept_Staff"], gr_gup);
                    }
                    O_DocClaim.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], mUser);

                    foreach (jFile file in remark.Files)
                    {
                        //string tdmsWordFilePath = System.IO.Path.Combine(file.Path);
                        //TDMSFile newFile = O_DocClaim.Files.Create("FILE_ALL", file.Path);
                        FilesString += file.Path + ";";
                        //ThisApplication.SaveContextObjects();
                    }
                    O_DocClaim.Attributes["A_Str_Files"].Value = FilesString;
                    ThisApplication.SaveContextObjects();
                }
                Logger.Info("GPPgetClaimRegistry: finished");
                response = "true";
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Info($"GPPgetClaimRegistry: finished with: {response}");
                return response;
            }
        }

        /* Flow 2 RZ */
        [Route("api/GPPgetClaimRegistry"), HttpPost]
        public string GPPgetClaimRegistry([FromBody] JsonPackageRZ jsonobject)
        {
            try
            {
                string response = "true";
                Logger.Info("Flow 2. GPPgetClaimRegistry: started");
                TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
                if (O_Package_Unload == null)
                {
                    response = $"Не найден 'Пакет выгрузки'{ jsonobject.O_Package_Unload.ToString() }";
                    Logger.Info(response);
                    return response;
                };

                Logger.Info($"Найден 'Пакет выгрузки' { jsonobject.O_Package_Unload.ToString() }");
                TDMSObject O_ClaimRegistry = ThisApplication.GetObjectByGUID(jsonobject.RZ.External_Guid.ToString());
                jUser UserInitiated;
                TDMSObject O_Document;
                TDMSUser A_User_Author;
                TDMSGroup gr_gup = null;

                if (O_ClaimRegistry == null)
                {
                    Logger.Info($"Не найден 'Реестр замечаний' { jsonobject.RZ.External_Guid }");
                    O_ClaimRegistry = O_Package_Unload.Objects.Create("O_ClaimRegistry");
                    Logger.Info($"Создан 'Реестр замечаний' { O_ClaimRegistry.GUID }");
                }
                else
                {
                    Logger.Info($"Найден 'Реестр замечаний' { jsonobject.RZ.External_Guid }");
                };

                O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value = jsonobject.RZ.Guid;
                O_ClaimRegistry.Attributes["A_Str_Name"].Value = jsonobject.RZ.ATTR_NAME_REGISTRY;
                O_ClaimRegistry.Attributes["A_Date_Create"].Value = DateTime.Parse(jsonobject.RZ.ATTR_REGYSTRY_CREATION_DATE);
                O_ClaimRegistry.Attributes["A_Str_Designation"].Value = jsonobject.RZ.ATTR_Registry_Num;
                O_ClaimRegistry.Attributes["A_Dat_Req_Deadline"].Value = jsonobject.RZ.ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE;
                UserInitiated = jsonobject.RZ.ATTR_Registry_UserInitiated;
                O_ClaimRegistry.Attributes["A_Str_ClaimAuthor"].Value = $"{UserInitiated.LastName} {UserInitiated.FirstName} {UserInitiated.MiddleName}, {UserInitiated.Tel}, {UserInitiated.Mail}";
                O_Document = ThisApplication.GetObjectByGUID(jsonobject.RZ.TD_External_Guid.ToString());
                O_ClaimRegistry.Attributes["A_Ref_Doc"].Value = O_Document;
                A_User_Author = O_Document.Attributes["A_User_Author"].User;

                if (A_User_Author != null)
                {
                    O_ClaimRegistry.Attributes["A_User_Author"].Value = A_User_Author;
                    O_ClaimRegistry.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], A_User_Author);

                    TDMSTableAttribute tdept = A_User_Author.Attributes["A_Table_Depts"].Rows;
                    if (tdept.Count > 0)
                    {
                        TDMSObject mdept = null;
                        foreach (TDMSTableAttributeRow row in tdept)
                        {
                            if (row.Attributes["A_Bool_MainDept"].Value.ToString() == "True")
                            {
                                mdept = row.Attributes["A_Ref_Dept"].Object;
                            }
                        }
                        if (mdept != null)
                        {
                            TDMSQuery qDeps = ThisApplication.Queries["Q_Dept_Users"];
                            qDeps.SetParameter("DeptObj", mdept);
                            string scode = mdept.Attributes["A_Str_NumCode"].Value.ToString().Replace(".", "_");

                            if (!ThisApplication.Groups.Has("G_" + scode))
                            {
                                TDMSGroup gr = ThisApplication.Groups.Create();
                                gr.SysName = "G_" + scode;
                                gr.Description = $"{mdept.Attributes["A_Str_NumCode"].Value} { mdept.Attributes["A_Str_Name"].Value}";
                                ThisApplication.SaveChanges();
                            }
                            gr_gup = ThisApplication.Groups["G_" + scode];

                            foreach (TDMSUser user in qDeps.Users)
                            {
                                gr_gup.Users.Add(user);
                            }
                            ThisApplication.SaveChanges();
                        }
                    }
                }
                else
                {
                    Logger.Info($"Не заполнен атрибут 'A_User_Author' в документе {O_Document.GUID}");
                };

                ThisApplication.SaveContextObjects();
                //string FilesString;


                foreach (jRemark remark in jsonobject.Remarks)
                {
                    //string FilesString;

                    TDMSObject O_DocClaim = ThisApplication.GetObjectByGUID(remark.External_Guid);
                    if (O_DocClaim == null)
                    {
                        Logger.Info($"Не найдено 'Замечание' {remark.External_Guid}");
                        String FilesString = "";

                        O_ClaimRegistry.Attributes["A_Bool_Started"].Value = false;
                        O_ClaimRegistry.Status = ThisApplication.Statuses["S_ClaimRegistry_Actual"];

                        O_DocClaim = O_ClaimRegistry.Objects.Create("O_DocClaim");
                        O_DocClaim.Attributes["A_Str_Designation"].Value = remark.ATTR_Remark_Num;
                        O_DocClaim.Attributes["A_Str_ClaimDesc"].Value = remark.ATTR_Remark;
                        //O_DocClaim.Attributes["A_Str_AnswerDesc"].Value = remark.ATTR_Answer;
                        //O_DocClaim.Attributes["A_Str_Answer"].Value = remark.ATTR_Answer_Type;
                        O_DocClaim.Attributes["A_Int_DocVersion"].Value = remark.ATTR_TechDoc_Version;
                        O_DocClaim.Attributes["A_Str_GUID_External"].Value = remark.Guid;
                        jUser authorZM = remark.ATTR_AUTHOR_ZM;
                        O_DocClaim.Attributes["A_Str_ClaimAuthor"].Value = $"{authorZM.LastName} {authorZM.FirstName} {authorZM.MiddleName}, {authorZM.Tel}, {authorZM.Mail}";
                        //O_DocClaim.Attributes["A_Date_Answer"].Value = remark.ATTR_Answer_Date;
                        O_DocClaim.Attributes["A_Date_Create"].Value = remark.ATTR_Remark_Date;
                        O_DocClaim.Attributes["A_Str_Claim"].Value = remark.ATTR_REMARK_TYPE;
                        O_DocClaim.Attributes["A_Ref_DocClaimRegistry"].Value = O_ClaimRegistry;
                        if (gr_gup != null)
                        {
                            O_DocClaim.Roles.Create(ThisApplication.RoleDefs["R_Dept_Staff"], gr_gup);
                        }
                        O_DocClaim.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], A_User_Author);
                        foreach (jFile file in remark.Files)
                        {
                            FilesString += file.Path + ";";
                        };
                        O_DocClaim.Attributes["A_Str_Files"].Value = FilesString;
                        Logger.Info($"Создано 'Замечание' {O_DocClaim.GUID}");
                    }
                    else
                    {
                        switch (remark.Status.Sysname)
                        {
                            case "STATUS_GETTING_A_RESPONSE_TO_ZM": //Выдано

                                String FilesString = "";
                                int i = Int32.Parse(O_DocClaim.VersionName) + 1;
                                O_DocClaim.CreateVersion(i, $"Создана на базе {O_DocClaim.VersionName}");
                                ThisApplication.SaveChanges();
                                //O_DocClaim.Update();
                                TDMSObject O_DocClaim_new = O_DocClaim.Versions.Active;
                                O_ClaimRegistry.Attributes["A_Bool_Started"].Value = false;
                                O_ClaimRegistry.Status = ThisApplication.Statuses["S_ClaimRegistry_Actual"];
                                O_DocClaim_new.Attributes["A_Bool_Started"].Value = false;
                                O_DocClaim_new.Attributes["A_Str_Designation"].Value = remark.ATTR_Remark_Num;
                                O_DocClaim_new.Attributes["A_Str_ClaimDesc"].Value = remark.ATTR_Remark;
                                O_DocClaim_new.Attributes["A_User_AnswerAuthor"].Value = "";
                                O_DocClaim_new.Attributes["A_Str_AnswerDesc"].Value = "";
                                O_DocClaim_new.Attributes["A_Str_Answer"].Value = "";
                                O_DocClaim_new.Attributes["A_Int_DocVersion"].Value = remark.ATTR_TechDoc_Version;
                                O_DocClaim_new.Attributes["A_Str_GUID_External"].Value = remark.Guid;
                                jUser authorZM = remark.ATTR_AUTHOR_ZM;
                                O_DocClaim_new.Attributes["A_Str_ClaimAuthor"].Value = $"{authorZM.LastName} {authorZM.FirstName} {authorZM.MiddleName}, {authorZM.Tel}, {authorZM.Mail}";
                                O_DocClaim_new.Attributes["A_Date_Answer"].Value = "";
                                O_DocClaim_new.Attributes["A_Date_Create"].Value = remark.ATTR_Remark_Date;
                                O_DocClaim_new.Attributes["A_Str_Claim"].Value = remark.ATTR_REMARK_TYPE;
                                O_DocClaim_new.Attributes["A_Ref_DocClaimRegistry"].Value = O_ClaimRegistry;
                                if (gr_gup != null)
                                {
                                    O_DocClaim_new.Roles.Create(ThisApplication.RoleDefs["R_Dept_Staff"], gr_gup);
                                };
                                O_DocClaim_new.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], A_User_Author);
                                foreach (jFile file in remark.Files)
                                {
                                    FilesString += file.Path + ";";
                                };
                                O_DocClaim_new.Attributes["A_Str_Files"].Value = FilesString;
                                Logger.Info("!!!!!");
                                O_DocClaim_new.Update();
                                Logger.Info(O_DocClaim_new.GUID);
                                O_DocClaim_new.Status = ThisApplication.Statuses["S_DocClaim_Actual"];
                                foreach (TDMSFile file in O_DocClaim_new.Files)
                                {
                                    O_DocClaim_new.Files.Remove(file);
                                };
                                ThisApplication.SaveChanges();
                                break;
                            case "STATUS_COMMENT_RESOLVED": //Устранено
                                O_DocClaim.Status = ThisApplication.Statuses["S_DocClaim_NotActual"];
                                ThisApplication.SaveContextObjects();

                                break;
                        }
                    };

                    /*
                    String FilesString = "";
                    TDMSObject O_DocClaim = O_ClaimRegistry.Objects.Create("O_DocClaim");
                    O_DocClaim.Attributes["A_Str_Designation"].Value = remark.ATTR_Remark_Num;
                    O_DocClaim.Attributes["A_Str_ClaimDesc"].Value = remark.ATTR_Remark;
                    O_DocClaim.Attributes["A_Str_AnswerDesc"].Value = remark.ATTR_Answer;
                    O_DocClaim.Attributes["A_Str_Answer"].Value = remark.ATTR_Answer_Type;
                    O_DocClaim.Attributes["A_Int_DocVersion"].Value = remark.ATTR_TechDoc_Version;
                    O_DocClaim.Attributes["A_Str_GUID_External"].Value = remark.Guid;

                    jUser authorZM = remark.ATTR_AUTHOR_ZM;
                    O_DocClaim.Attributes["A_Str_ClaimAuthor"].Value = $"{authorZM.LastName} {authorZM.FirstName} {authorZM.MiddleName}, {authorZM.Tel}, {authorZM.Mail}";

                    //jUser authorAnswer = remark.ATTR_AUTHOR_ANSWER;
                    //O_DocClaim.Attributes["A_User_AnswerAuthor"].Value = $"{authorAnswer.LastName} {authorAnswer.FirstName} {authorAnswer.MiddleName}, {authorAnswer.Tel}, {authorAnswer.Mail}";

                    O_DocClaim.Attributes["A_Date_Answer"].Value = remark.ATTR_Answer_Date;
                    O_DocClaim.Attributes["A_Date_Create"].Value = remark.ATTR_Remark_Date;
                    O_DocClaim.Attributes["A_Str_Claim"].Value = remark.ATTR_REMARK_TYPE;
                    O_DocClaim.Attributes["A_Ref_DocClaimRegistry"].Value = O_ClaimRegistry;
                    //O_DocClaim.Attributes["A_User_Author"].Value = mUser;
                    if (gr_gup != null)
                    {
                        O_DocClaim.Roles.Create(ThisApplication.RoleDefs["R_Dept_Staff"], gr_gup);
                    }
                    O_DocClaim.Roles.Create(ThisApplication.RoleDefs["ROLE_DEVELOPER"], A_User_Author);

                    foreach (jFile file in remark.Files)
                    {
                        //string tdmsWordFilePath = System.IO.Path.Combine(file.Path);
                        //TDMSFile newFile = O_DocClaim.Files.Create("FILE_ALL", file.Path);
                        FilesString += file.Path + ";";
                        //ThisApplication.SaveContextObjects();
                    }
                    O_DocClaim.Attributes["A_Str_Files"].Value = FilesString;
                    */
                    ThisApplication.SaveContextObjects();
                }
                Logger.Info("Flow 2. GPPgetClaimRegistry: finished");
                return response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Info($"Flow 2. GPPgetClaimRegistry: finished with: {response}");
                return response;
            }
        }

        /* Flow 3.1 STATUS DOC OR RZ*/
        [Route("api/GPPgetAnswersZMresponse"), HttpPost]
        public string GPPgetAnswersZMresponse([FromBody] ResponseJson jsonobjectO)
        {
            try
            {
                Logger.Info("GPPgetAnswersZMresponse: started");
                String textmessage = "";
                TDMSObject RZ = ThisApplication.GetObjectByGUID(jsonobjectO.Objects[0].ObjGuidExternal);
                if (jsonobjectO.Completed)
                {
                    if (jsonobjectO.Objects[0].ObjStatus == "STATUS_ANALYSIS_OF_RESPONSES")
                    {
                        textmessage = $"Реестр замечаний \"{RZ.Attributes["A_Str_Designation"].Value}\" успешно принят";
                    }
                    else
                    {
                        textmessage = $"Реестр замечаний \"{RZ.Attributes["A_Str_Designation"].Value}\" не принят. Попробуйте еще раз позднее";
                    }
                }

                //var Functions = new Functions(ThisApplication);
                Functionso.SendTDMSMessage(ThisApplication, textmessage, textmessage, RZ.Attributes["A_User_Author"].User);

                ThisApplication.SaveChanges();
                ThisApplication.SaveContextObjects();
                response = "true";
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + "\n" + ex.StackTrace);
                response = ex.Message + "\n" + ex.StackTrace;
            }
            return response;
        }

        /* Flow 4 STATUS CHANGE */
        [Route("api/ObjectsStatusChange"), HttpPost]
        public string ObjectsStatusChange([FromBody] ResponseJson jsonobjectO)
        {
            try
            {
                Logger.Info("ObjectsStatusChange: started");
                //var Functions = new Functions(ThisApplication);
                var objects = jsonobjectO.Objects;
                String response = "true";

                foreach (jObject jObj in objects)
                {
                    TDMSObject tdmsObject = ThisApplication.GetObjectByGUID(jObj.ObjGuidExternal);
                    if (tdmsObject != null)
                    {
                        switch (jObj.ObjStatus)
                        {
                            case "STATUS_TechDoc_InUse":
                                /* Всем РЗ и ЗМ ставим статус "Не актуально" */
                                TDMSObjects RZs = tdmsObject.ReferencedBy.ObjectsByDef("O_ClaimRegistry");
                                foreach (TDMSObject O_ClaimRegistry in RZs)
                                {
                                    O_ClaimRegistry.Status = ThisApplication.Statuses["S_ClaimRegistry_NotActual"];
                                    foreach (TDMSObject O_DocClaim in O_ClaimRegistry.Objects)
                                    {
                                        O_DocClaim.Status = ThisApplication.Statuses["S_DocClaim_NotActual"];
                                    }
                                }

                                /* Находим Пакет выгрузки связанный с этим Доком*/
                                TDMSObject O_Package_Unload = tdmsObject.Attributes["A_Ref_Package_Unload"].Object;

                                /* В табличном атрибуте мменяем статус напротив Дока */
                                TDMSTableAttribute Rows = O_Package_Unload.Attributes["A_Table_DocReview"].Rows;
                                foreach (TDMSTableAttributeRow row in Rows)
                                {
                                    TDMSObject rowObject = row.Attributes["A_Ref_Doc"].Object;
                                    if (rowObject.InternalObject.ObjectGuid == tdmsObject.InternalObject.ObjectGuid)
                                    {
                                        row.Attributes["A_Cls_DocReviewStatus"].Value = ThisApplication.Classifiers["N_DocReview_Status"].Classifiers["N_DocReview_Status_Actual"];
                                        break;
                                    }
                                }
                                ThisApplication.SaveChanges();
                                ClosePackageUnload(O_Package_Unload);
                                TDMSUser tdmsUser = tdmsObject.Attributes["A_User_Author"].User;
                                String text = $"Документ \"{tdmsObject.Attributes["A_Str_Designation"].Value}\" введен в действие";
                                Functionso.SendTDMSMessage(ThisApplication, text, text, tdmsUser);
                                //response = "true";
                                break;

                            case "STATUS_TechDoc_Annulated":
                                /* Всем РЗ и ЗМ ставим статус "Не актуально" */
                                RZs = tdmsObject.ReferencedBy.ObjectsByDef("O_ClaimRegistry");
                                foreach (TDMSObject O_ClaimRegistry in RZs)
                                {
                                    O_ClaimRegistry.Status = ThisApplication.Statuses["S_ClaimRegistry_NotActual"];
                                    foreach (TDMSObject O_DocClaim in O_ClaimRegistry.Objects)
                                    {
                                        O_DocClaim.Status = ThisApplication.Statuses["S_DocClaim_NotActual"];
                                    }
                                }

                                /* Находим Пакет выгрузки связанный с этим Доком*/
                                O_Package_Unload = tdmsObject.Attributes["A_Ref_Package_Unload"].Object;

                                /* В табличном атрибуте мменяем статус напротив Дока */
                                Rows = O_Package_Unload.Attributes["A_Table_DocReview"].Rows;
                                foreach (TDMSTableAttributeRow row in Rows)
                                {
                                    TDMSObject rowObject = row.Attributes["A_Ref_Doc"].Object;
                                    if (rowObject.InternalObject.ObjectGuid == tdmsObject.InternalObject.ObjectGuid)
                                    {
                                        row.Attributes["A_Cls_DocReviewStatus"].Value = ThisApplication.Classifiers["N_DocReview_Status"].Classifiers["N_DocReview_Status_Annul"];
                                        break;
                                    }
                                }
                                ThisApplication.SaveChanges();
                                ClosePackageUnload(O_Package_Unload);
                                tdmsUser = tdmsObject.Attributes["A_User_Author"].User;
                                text = $"Документ \"{tdmsObject.Attributes["A_Str_Designation"].Value}\" аннулирован";
                                Functionso.SendTDMSMessage(ThisApplication, text, text, tdmsUser);
                                response = "true";
                                break;

                            case "STATUS_REVIEW_COMPLETED":
                                /* Всем РЗ и ЗМ ставим статус "Не актуально" */
                                tdmsObject.Status = ThisApplication.Statuses["S_ClaimRegistry_NotActual"];
                                foreach (TDMSObject O_DocClaim in tdmsObject.Objects)
                                {
                                    O_DocClaim.Status = ThisApplication.Statuses["S_DocClaim_NotActual"];
                                }
                                ThisApplication.SaveChanges();
                                tdmsUser = tdmsObject.Attributes["A_User_Author"].User;
                                text = $"Реестр замечаний \"{tdmsObject.Attributes["A_Str_Designation"].Value}\" закрыт";
                                Functionso.SendTDMSMessage(ThisApplication, text, text, tdmsUser);
                                //response = "true";
                                break;

                            default:
                                if (response != "true")
                                {
                                    response = "false";
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (jObj.ObjDefName == "OBJECT_Technical_Doc")
                        {
                            response = "false";
                        }
                    }
                }
                #region old code
                //TDMSObject project = ThisApplication.GetObjectByGUID(jsonobjectO.Objects[0].ObjGuidExternal);
                //if (jsonobjectO.Completed)
                //{
                //    if (jsonobjectO.Objects[0].ObjStatus == "STATUS_Prj_InProgress")
                //    {
                //        textmessage = $"Проект \"{project.Attributes["A_Str_Designation"].Value}\" успешно запущен";
                //    }
                //}
                //var Functions = new Functions(ThisApplication);
                //Functions.SendTDMSMessage(textmessage, textmessage, project.Attributes["A_User_GIP"].User);
                #endregion

                ThisApplication.SaveChanges();
                ThisApplication.SaveContextObjects();
                Logger.Info("ObjectsStatusChange: finished");
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + "\n" + ex.StackTrace);
                response = ex.Message + "\n" + ex.StackTrace;
            }
            return response;
        }

        /* ЗАКРЫТИЕ ПВ */
        public void ClosePackageUnload(TDMSObject O_Package_Unload)
        {
            TDMSTableAttribute Rows = O_Package_Unload.Attributes["A_Table_DocReview"].Rows;
            bool result = true;
            foreach (TDMSTableAttributeRow row in Rows)
            {
                if (row.Attributes["A_Cls_DocReviewStatus"].Classifier.SysName == ThisApplication.Classifiers["N_DocReview_Status"].Classifiers["N_DocReview_Status_OnReview"].SysName)
                {
                    result = false;
                    break;
                }
            };
            if (result)
            {
                Logger.Info("result = true");
                O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_Closed"];
            }
        }
        //4 поток - api/ObjectsStatusChange метод POST
    }
}

/* Restricted
    private static System.Timers.Timer aTimer;
    private void SetTimer()
    {
        aTimer = new System.Timers.Timer(2000);
        aTimer.Elapsed += OnTimedEvent;
        aTimer.Interval = 5000;
        aTimer.Enabled = true;
    }

    public void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        Logger.Info("rrr");
    }
*/

/* Variant with abstract class
[TdmsApi("ShTask")]
public class ShTask : WebCommand
{
    public ShTasko(TDMSApplication application)
    {
        ThisApplication = application;
        Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
    }

    public ShTask(TDMSApplication thisApplication, TDMSObject thisObject) : base(thisApplication, thisObject)
    {
        TDMSApplication application;
        //Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");

        Logger.Info("ddd");
    }
    public void Execute()
    {
        Logger.Info("eee");
    }

    private static System.Timers.Timer aTimer;
    private static void SetTimer()
    {
        aTimer = new System.Timers.Timer(2000);
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    public static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
        Logger.Info("rrr");
    }
}

public abstract class WebCommand
{
    public ILogger Logger { get; set; }

    protected TDMSApplication ThisApplication;
    protected TDMSObject ThisObject;
    protected TDMSPermissions SysadminPermissions;

    public WebCommand(TDMSApplication app, TDMSObject thisObject)
    {
        ThisApplication = app;
        SysadminPermissions = TDMSPermissions.GetSysadminPermissions(app.Context);
        ThisObject = thisObject;
    }
}
*/