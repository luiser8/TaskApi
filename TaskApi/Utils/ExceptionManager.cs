using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace TaskApi.Utils
{
    /// <summary>
    /// Clase que tiene los helpers para administrar el logging de excepciones
    /// </summary>
    public static class ExceptionManager
    {
        #region Server Exceptions
        public static void Validation(int code, string message)
        {
            Validation(code, message, "");
        }
        [DebuggerStepThrough]
        public static void Validation(int code, string message, string tracepath)
        {
            throw new ValidationException(code, message);
        }

        #endregion

        #region Handling Exceptions

        public static System.Web.Http.Results.ResponseMessageResult ReportException(System.Web.Http.ApiController api, Exception ex)
        {
            return ReportException(api, ex, ConfigurationManager.AppSettings["ErrorLogPath"]);
        }

        public static System.Web.Http.Results.ResponseMessageResult ReportException(System.Web.Http.ApiController api, Exception ex, string ErrorLogFilesFolder)
        {
            string ErrorLogFile = "";
            string ClientCode = "";

            if(ex is ValidationException)
            {
                return new System.Web.Http.Results.ResponseMessageResult(
                    api.Request.CreateResponse(HttpStatusCode.InternalServerError, ex)
                    );
            }
            

            // si viniera vacío, es que no pudo leer de la configuración una ruta específica
            // automàticamente redirecciona a una carpeta autocreada
            if (string.IsNullOrEmpty(ErrorLogFilesFolder))
            {
                ErrorLogFilesFolder = AppDomain.CurrentDomain.BaseDirectory + "\\ErrorLogFiles";
            }

            // Verificaciòn de existencia del directorio
            if (!System.IO.Directory.Exists(ErrorLogFilesFolder))
                System.IO.Directory.CreateDirectory(ErrorLogFilesFolder);

            // Creaciòn del nombre del archivo de log
            while (true)
            {
                //string Prefix = source == ExceptionSources.Server ? "S_" : "C_";
                string Token = Guid.NewGuid().ToString();
                ClientCode = Token.Substring(Token.Length - 6, 6);

                ErrorLogFile = ErrorLogFilesFolder + ClientCode + ".log";

                if (!System.IO.File.Exists(ErrorLogFile))
                    break;
            }

            // Conversión de excepción a texto para loguearse
            string SerializedException = SerializeException(ex);

            // Escribe el archivo de log
            System.IO.File.WriteAllText(ErrorLogFile, SerializedException, System.Text.Encoding.Default);

            //No se implementó el envío de mail de los logs, debido a que hay una pantalla para revisarlos
            //Envío de mail a través de la cola de Mensajería
            //Enviar al servidor el error para guardarlo

            return new System.Web.Http.Results.ResponseMessageResult(
                    api.Request.CreateResponse(HttpStatusCode.InternalServerError, 
                    new Exception("Se ha producido un error favor contactese con su administrador con el código: " + ClientCode)));
        }

        public static string SerializeException(Exception ex)
        {
            StringBuilder error = new StringBuilder();

            error.AppendLine("Date:              " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            error.AppendLine("OS:                " + Environment.OSVersion.ToString());
            error.AppendLine("Culture:           " + CultureInfo.CurrentCulture.Name);
            error.AppendLine("App up time:       " + (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());

            try
            {
                Exception exception = ex;

                while (true)
                {
                    if (exception == null) break;
                    error.AppendLine("|||||||||||||||||||||||||||||||||||");
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    error.AppendLine(exception.Message);
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    error.AppendLine(exception.StackTrace);
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    error.AppendLine(exception.Source);
                    if (exception.TargetSite != null)
                        error.AppendLine(exception.TargetSite.ToString());
                    error.AppendLine(exception.HelpLink);
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                    error.AppendLine("Exception classes:   ");
                    error.AppendLine(GetExceptionTypeStack(exception));
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    error.AppendLine("Exception messages: ");
                    error.AppendLine(GetExceptionMessageStack(exception));

                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    error.AppendLine("Stack Traces:");
                    error.AppendLine(GetExceptionCallStack(exception));
                    error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                    if (exception is DbEntityValidationException)
                    {
                        var dbex = (DbEntityValidationException)exception;
                        if (dbex != null)
                        {
                            error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            error.AppendLine("Entity Validation Exception:");
                            foreach (var validation in dbex.EntityValidationErrors)
                            {
                                if (validation != null)
                                {
                                    if (validation.Entry != null)
                                    {
                                        error.AppendLine(validation.Entry.Entity.ToString());
                                    }
                                    error.AppendLine("IsValid: " + validation.IsValid.ToString());
                                    if (validation.ValidationErrors != null)
                                    {
                                        foreach (var err in validation.ValidationErrors)
                                        {
                                            error.AppendLine("PropertyName: " + err.PropertyName + ", Msg: " + err.ErrorMessage);

                                            error.AppendLine(string.Format("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                               err.PropertyName,
                                               validation.Entry.CurrentValues.GetValue<object>(err.PropertyName),
                                               err.ErrorMessage));
                                        }
                                    }
                                }
                            }

                            error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        }
                    }

                    if (exception is SqlException)
                    {
                        var sqlex = (SqlException)exception;
                        if (sqlex != null)
                        {
                            error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                            error.AppendLine("SQL Exception:");
                            error.AppendLine("ConnectionID: " + sqlex.ClientConnectionId);
                            for (int i = 0; i < sqlex.Errors.Count; i++)
                            {
                                error.AppendLine("Index #" + i + "\n" +
                                    "Message: " + sqlex.Errors[i].Message + "\n" +
                                    "LineNumber: " + sqlex.Errors[i].LineNumber + "\n" +
                                    "Source: " + sqlex.Errors[i].Source + "\n" +
                                    "Procedure: " + sqlex.Errors[i].Procedure + "\n");
                            }
                            error.AppendLine("ConnectionID: " + sqlex.Errors);

                            error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        }
                    }

                    if (exception is System.ServiceModel.FaultException<System.ServiceModel.ExceptionDetail>)
                    {
                        System.ServiceModel.FaultException<System.ServiceModel.ExceptionDetail> readed = (System.ServiceModel.FaultException<System.ServiceModel.ExceptionDetail>)exception;

                        error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        error.AppendLine(readed.Detail.Message);
                        error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        error.AppendLine(readed.Detail.HelpLink);
                        error.AppendLine(readed.Detail.StackTrace);
                        error.AppendLine(readed.Detail.Type);
                        error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                        ExceptionDetail exdetail = readed.Detail.InnerException;
                        if (exdetail != null)
                        {
                            while (true)
                            {
                                error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                                error.AppendLine(exdetail.Message);
                                error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                                error.AppendLine(exdetail.HelpLink);
                                error.AppendLine(exdetail.StackTrace);
                                error.AppendLine(exdetail.Type);
                                error.AppendLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                                exdetail = exdetail.InnerException;
                                if (exdetail == null)
                                    break;
                            }
                        }
                    }

                    exception = exception.InnerException;
                    if (exception == null) break;
                }

                error.AppendLine("");
                error.AppendLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
                //error.AppendLine("Loaded Modules:");
                //Process thisProcess = Process.GetCurrentProcess();
                //foreach (ProcessModule module in thisProcess.Modules)
                //{
                //    error.AppendLine(module.FileName + " " + module.FileVersionInfo.FileVersion);
                //}
            }
            catch (Exception inex)
            {
                error.AppendLine("Error en serialización de excepción: " + inex.Message);
            }

            return error.ToString();
        }

        public enum ExceptionSources { Server, Client }

        #region Support
        private static string GetExceptionTypeStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionTypeStack(e.InnerException));
                message.AppendLine("   " + e.GetType().ToString());
                return (message.ToString());
            }
            else
            {
                return "   " + e.GetType().ToString();
            }
        }
        private static string GetExceptionTypeStack(ExceptionDetail e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionTypeStack(e.InnerException));
                message.AppendLine("   " + e.GetType().ToString());
                return (message.ToString());
            }
            else
            {
                return "   " + e.GetType().ToString();
            }
        }
        private static string GetExceptionMessageStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionMessageStack(e.InnerException));
                message.AppendLine("   " + e.Message);
                return (message.ToString());
            }
            else
            {
                return "   " + e.Message;
            }
        }
        private static string GetExceptionMessageStack(ExceptionDetail e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionMessageStack(e.InnerException));
                message.AppendLine("   " + e.Message);
                return (message.ToString());
            }
            else
            {
                return "   " + e.Message;
            }
        }
        private static string GetExceptionCallStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionCallStack(e.InnerException));
                message.AppendLine("--- Next Call Stack:");
                message.AppendLine(e.StackTrace);
                return (message.ToString());
            }
            else
            {
                return e.StackTrace;
            }
        }
        private static string GetExceptionCallStack(ExceptionDetail e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionCallStack(e.InnerException));
                message.AppendLine("--- Next Call Stack:");
                message.AppendLine(e.StackTrace);
                return (message.ToString());
            }
            else
            {
                return e.StackTrace;
            }
        }
        #endregion
        #endregion
    }
    
    /// <summary>
    /// Clase que hace referencia a Excepciones de validación
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public ValidationException(int code, string Message)
            : base(Message + ":" + code.ToString())
        {
        }
    }
}
