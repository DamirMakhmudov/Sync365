using Microsoft.AspNetCore.Mvc;
using Tdms.Api;
using Tdms.Log;
using System.IO;
using System.Text;
using System.Reflection;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Timers;

namespace Sync365
{
    [TdmsApi("ShTask")]
    public class ShTask
    {
        TDMSApplication Application;
        public ILogger Logger { get; set; }

        public ShTask(TDMSApplication app)
        {
            Application = app;
        }
        public void Execute()
        {
            SetTimer();
            Logger.Info("eee");
        }

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

    }
    //[TdmsApi("ShTask")]
    //public class ShTask : WebCommand
    //{
    //    public ShTasko(TDMSApplication application)
    //    {
    //        ThisApplication = application;
    //        Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
    //    }

    //    public ShTask(TDMSApplication thisApplication, TDMSObject thisObject) : base(thisApplication, thisObject)
    //    {
    //        TDMSApplication application;
    //        //Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");

    //        Logger.Info("ddd");
    //    }
    //    public void Execute()
    //    {
    //        Logger.Info("eee");
    //    }

    //    //private static System.Timers.Timer aTimer;
    //    //private static void SetTimer()
    //    //{
    //    //    aTimer = new System.Timers.Timer(2000);
    //    //    aTimer.Elapsed += OnTimedEvent;
    //    //    aTimer.AutoReset = true;
    //    //    aTimer.Enabled = true;
    //    //}

    //    //public static void OnTimedEvent(Object source, ElapsedEventArgs e)
    //    //{
    //    //    //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
    //    //    Logger = Tdms.Log.LogManager.GetLogger("Sync365WebApi");
    //    //    Logger.Info("rrr");
    //    //}
    //}


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
                    Logger.Info("cool");
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
                response = "cool1";
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
        public string GPPgetClaimRegistry([FromBody] JsonObject jsonobjectO)
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

                TDMSObject O_Document = ThisApplication.GetObjectByGUID(jsonobject.RZ.ToString());
                O_ClaimRegistry.Attributes["A_Ref_Doc"].Value = O_Document;
                ThisApplication.SaveContextObjects();

                foreach (jRemark remark in jsonobject.Remarks)
                {
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
                    O_DocClaim.Attributes["A_Str_ClaimAuthor"].Value = O_ClaimRegistry;

                    
                    foreach (jFile file in remark.Files)
                    {
                        string tdmsWordFilePath = System.IO.Path.Combine(file.Path);
                        TDMSFile newFile = O_DocClaim.Files.Create("FILE_ALL", file.Path);
                        //O_DocClaim.SaveChanges(TDMSSaveOptions.tdmSaveOptUpdateDefault);
                        ThisApplication.SaveContextObjects();
                    }
                }

                //file.CheckOut(tdmsWordFilePath);
                //thisobject.Update();
                //thisobject.SaveChanges(TDMSSaveOptions.tdmSaveOptUpdateDefault);

                response = "successful";
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + "\n" + ex.StackTrace);
                response = ex.Message + "\n" + ex.StackTrace;
            }

            //TDMSObject O_Package_Unload = ThisApplication.GetObjectByGUID(jsonobject.O_Package_Unload.ToString());
            //TDMSAttributes Attrs = O_Package_Unload.Attributes;
            //if (jsonobject.Completed)
            //{
            //    Attrs["A_Bool_Load"].Value = true;
            //    Attrs["A_Str_GUID_External"].Value = jsonobject.FolderGuid;
            //    Attrs["A_Date_Load"].Value = DateTime.Now;
            //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_OnReview"];
            //}
            //else
            //{
            //    O_Package_Unload.Status = ThisApplication.Statuses["S_Package_Unload_Cancel"];
            //}
            //O_Package_Unload.Attributes["A_Bool_Load"].Value = true;
            //string taskText = jsonobject.task.ToString().ToLower();
            //var req = this.Request;

            ThisApplication.SaveContextObjects();
            return response;
        }

        
    }
}
