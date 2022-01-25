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
    /* SCHEDULER */
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
            try
            {
                Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
                Logger.Info("Execute started");

                TDMSQuery qO_ClaimRegistry = ThisApplication.CreateQuery();
                qO_ClaimRegistry.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, "O_ClaimRegistry");
                qO_ClaimRegistry.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "= Null or =''", "A_Bool_Started");

                Logger.Info("_ClaimRegistry conunt: " + qO_ClaimRegistry.Objects.Count.ToString());

                foreach (TDMSObject O_ClaimRegistry in qO_ClaimRegistry.Objects)
                {
                    Logger.Info("Done");
                    O_ClaimRegistry.Attributes["A_Bool_Started"].Value = true;
                }

                foreach (TDMSObject O_ClaimRegistry in qO_ClaimRegistry.Objects)
                {
                    try
                    {
                        Logger.Info(O_ClaimRegistry.Description);
                        TDMSQuery qO_DocClaim = ThisApplication.CreateQuery();
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, "O_DocClaim");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, O_ClaimRegistry, "A_Ref_Parent");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "= Null or =''", "A_Bool_Started");
                        qO_DocClaim.AddCondition(TDMSQueryConditionType.tdmQueryConditionAttribute, "<> ''", "A_Str_Files");

                        Logger.Info("O_DocClaim: " + qO_DocClaim.Objects.Count.ToString());

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
                                    //    //ThisApplication.SaveContextObjects();
                                }
                            }
                        }

                        ResponseJson rjsonobject = new ResponseJson();
                        rjsonobject.SystemName = systemname;
                        rjsonobject.Result = "true";
                        rjsonobject.Date = DateTime.Now.ToString();
                        rjsonobject.ObjGuidExternal = O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value.ToString();
                        rjsonobject.ObjGuid = O_ClaimRegistry.GUID;
                        rjsonobject.Completed = true;
                        var json = System.Text.Json.JsonSerializer.Serialize(rjsonobject);

                        var data = sendR(json);
                        Logger.Info(data);
                        ThisApplication.SaveChanges();

                        TDMSObject O_Package_Unload = O_ClaimRegistry.Parent;
                        TDMSObject O_Project = O_Package_Unload.Attributes["A_Ref_Project"].Object;
                        TDMSMessage Msg = ThisApplication.CreateMessage();
                        Msg.Subject = $"Реестр замечаний: \"{O_Package_Unload.Description}\"";
                        Msg.Body = $"Получен реестр замечаний \"{O_ClaimRegistry.Description}\" по следующему пакету загрузки \"{O_Package_Unload.Description}\"";
                        Msg.ToAdd(O_Package_Unload.Attributes["A_User_Author"].User);
                        Msg.System = false;
                        Msg.Send();
                    }
                    catch(Exception ex)
                    {
                        ResponseJson rjsonobject = new ResponseJson();
                        rjsonobject.SystemName = systemname;
                        rjsonobject.Result = ex.Message + "\n" + ex.StackTrace;
                        rjsonobject.Date = DateTime.Now.ToString();
                        rjsonobject.ObjGuidExternal = O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value.ToString();
                        rjsonobject.ObjGuid = O_ClaimRegistry.GUID;
                        rjsonobject.Completed = false;
                        var json = System.Text.Json.JsonSerializer.Serialize(rjsonobject);
                        var data = sendR(json);
                        Logger.Error(data);
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Error(response);
            }
        }

        public string sendR(string json)
        {
            var url = ThisApplication.Attributes["a_url_365"].Value + "/api/GPPimportRZstatus";
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

        /* Flow 0.1 */
        [Route("api/GPPtransferProjectResponse"), HttpPost]
        public string GPPtransferProjectResponse([FromBody] JsonObject jsonobjectO)
        {
            try
            {
                Logger.Info("GPPtransferProjectResponse: started");
                jsonobject = jsonobjectO;

                //TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
                //TDMSAttributes Attrs = O_Package_Unload.Attributes;
                //if (jsonobject.Completed)
                //{
                //    Attrs["A_Bool_Load"].Value = true;
                //    Attrs["A_Str_GUID_External"].Value = jsonobject.FolderGuid;
                //    Attrs["A_Date_Load"].Value = DateTime.Now;
                //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_OnReview"];
                //    Logger.Info("true");
                //}
                //else
                //{
                //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_Cancel"];
                //    TDMSMessage Msg = ThisApplication.CreateMessage();
                //    Msg.Subject = "Ошибка при импорте пакета \"" + O_Package_Unload.Description + "\"";
                //    Msg.Body = jsonobject.Result.ToString();
                //    Msg.ToAdd(ThisApplication.CurrentUser);
                //    Msg.System = false;
                //    Msg.Send();
                //    Logger.Info("some error");
                //}

                //O_Package_Unload.Attributes["A_Bool_Load"].Value = true;
                //string taskText = jsonobject.task.ToString().ToLower();
                //var req = this.Request;

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

        /* Flow 0.2 */
        [Route("api/GPPtransferProjectResponse2"), HttpPost]
        public string GPPtransferProjectResponse2([FromBody] JsonObject jsonobjectO)
        {
            try
            {
                Logger.Info("GPPtransferProjectResponse2: started");
                jsonobject = jsonobjectO;

                //TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
                //TDMSAttributes Attrs = O_Package_Unload.Attributes;
                //if (jsonobject.Completed)
                //{
                //    Attrs["A_Bool_Load"].Value = true;
                //    Attrs["A_Str_GUID_External"].Value = jsonobject.FolderGuid;
                //    Attrs["A_Date_Load"].Value = DateTime.Now;
                //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_OnReview"];
                //    Logger.Info("true");
                //}
                //else
                //{
                //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_Cancel"];
                //    TDMSMessage Msg = ThisApplication.CreateMessage();
                //    Msg.Subject = "Ошибка при импорте пакета \"" + O_Package_Unload.Description + "\"";
                //    Msg.Body = jsonobject.Result.ToString();
                //    Msg.ToAdd(ThisApplication.CurrentUser);
                //    Msg.System = false;
                //    Msg.Send();
                //    Logger.Info("some error");
                //}

                //O_Package_Unload.Attributes["A_Bool_Load"].Value = true;
                //string taskText = jsonobject.task.ToString().ToLower();
                //var req = this.Request;

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

        /* Flow 1.1 */
        [Route("api/GPPtransferDocResponse"), HttpPost]
        public string GPPtransferDocResponse([FromBody] JsonObject jsonobjectO)
        {
            try
            {
                Logger.Info("GPPtransferDocResponse: started");
                jsonobject = jsonobjectO;
                TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
                TDMSAttributes Attrs = O_Package_Unload.Attributes;
                if (jsonobject.Completed)
                {
                    Attrs["A_Bool_Load"].Value = true;
                    Attrs["A_Str_GUID_External"].Value = jsonobject.FolderGuid;
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
                //O_Package_Unload.Attributes["A_Bool_Load"].Value = true;
                //string taskText = jsonobject.task.ToString().ToLower();
                //var req = this.Request;
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

        /* Flow 2 */
        [Route("api/GPPgetClaimRegistry"), HttpPost]
        public String GPPgetClaimRegistry([FromBody] JsonObject jsonobjectO)
        {
            try
            {
                Logger.Info("GPPgetClaimRegistry: started");
                jsonobject = jsonobjectO;
                TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());

                TDMSObject O_ClaimRegistry = O_Package_Unload.Objects.Create("O_ClaimRegistry");
                O_ClaimRegistry.Attributes["A_Str_GUID_External"].Value = jsonobject.RZ.Guid;
                O_ClaimRegistry.Attributes["A_Str_Name"].Value = jsonobject.RZ.ATTR_NAME_REGISTRY;
                O_ClaimRegistry.Attributes["A_Date_Create"].Value = DateTime.Parse(jsonobject.RZ.ATTR_REGYSTRY_CREATION_DATE);
                O_ClaimRegistry.Attributes["A_Str_Designation"].Value = jsonobject.RZ.ATTR_Registry_Num;
                O_ClaimRegistry.Attributes["A_Dat_Req_Deadline"].Value = jsonobject.RZ.ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE;
                O_ClaimRegistry.Attributes["A_Str_ClaimAuthor"].Value = jsonobject.RZ.ATTR_Registry_UserInitiated;
                
                TDMSObject O_Document = ThisApplication.GetObjectByGUID(jsonobject.RZ.TD_External_Guid.ToString());
                O_ClaimRegistry.Attributes["A_Ref_Doc"].Value = O_Document;
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
                    O_DocClaim.Attributes["A_Str_ClaimAuthor"].Value = remark.ATTR_AUTHOR_ZM;
                    O_DocClaim.Attributes["A_User_AnswerAuthor"].Value = remark.ATTR_AUTHOR_ANSWER;
                    O_DocClaim.Attributes["A_Date_Answer"].Value = remark.ATTR_Answer_Date;
                    O_DocClaim.Attributes["A_Date_Create"].Value = remark.ATTR_Remark_Date;
                    O_DocClaim.Attributes["A_Str_Claim"].Value = remark.ATTR_REMARK_TYPE;
                    O_DocClaim.Attributes["A_Ref_DocClaimRegistry"].Value = O_ClaimRegistry;
                    
                    foreach (jFile file in remark.Files)
                    {
                        //string tdmsWordFilePath = System.IO.Path.Combine(file.Path);
                        //TDMSFile newFile = O_DocClaim.Files.Create("FILE_ALL", file.Path);
                        FilesString += file.Path + ";";
                        //ThisApplication.SaveContextObjects();
                    }
                    O_DocClaim.Attributes["A_Str_Files"].Value = FilesString;
                }

                response = "true";
            }
            catch (Exception ex)
            {
                response = ex.Message + "\n" + ex.StackTrace;
                Logger.Error(response);
            }
            ThisApplication.SaveContextObjects();
            return response;
        }
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