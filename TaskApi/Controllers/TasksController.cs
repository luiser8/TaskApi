using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TaskApi.Models;
using TaskApi.Utils;

namespace TaskApi
{
    public class TasksController : ApiController
    {
        /// <summary>
        /// Devuelve una lista JSON de la tabla Task
        /// </summary>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // GET: api/Tasks
        public IHttpActionResult GetTasks()
        {
            try
            {
                using (TasksEFModel db = new TasksEFModel())
                {
                    return Ok(db.Tasks.ToList());
                }
            }
            catch (Exception ex)
            {
                return ExceptionManager.ReportException(this, ex);
            }
        }
        /// <summary>
        /// Devuelve una lista JSON de la tabla Tasks
        /// </summary>
        /// <param name="user"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // GET: api/Tasks/5
        [ResponseType(typeof(Tasks))]
        public async Task<IHttpActionResult> GetTasks(int user)
        {
            if (user != 0)
            {
                try
                {
                    using (TasksEFModel db = new TasksEFModel())
                    {
                        var tasks = await db.Tasks.Where(u => u.IdUser == user).ToListAsync();
                        if (tasks == null)
                        {
                            ExceptionManager.Validation(10000, "Tarea del usuario no encontrado");
                        }
                        return Ok(tasks);
                    }
                }
                catch (Exception ex)
                {
                    return ExceptionManager.ReportException(this, ex);
                }
            }
            return Ok();
        }

        /// <summary>
        /// Metodo PUT, Hacemos actualizacion de la tabla Tasks
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tasks"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // PUT: api/Tasks/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTasks(int id, Tasks tasks)
        {
            if (!ModelState.IsValid)
            {
                ExceptionManager.Validation(10001, "Se deben enviar todos los atributos del modelo de la tarea");
            }

            if (id != tasks.IdTask)
            {
                ExceptionManager.Validation(10002, "EL id de tarea debe coincidir para realizar el update");
            }

            using (TasksEFModel db = new TasksEFModel())
            {
                tasks.CreateTask = DateTime.Now;
                db.Entry(tasks).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!TasksExists(id))
                    {
                        ExceptionManager.Validation(10003, "Error de Concurrencia en la tarea");
                    }
                    else
                    {
                        ExceptionManager.ReportException(this, ex);
                    }
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Metodo POST, recibe el request del form con los datos para crear un nuevo registro en la tabla Tasks
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // POST: api/Tasks
        [ResponseType(typeof(Tasks))]
        public async Task<IHttpActionResult> PostTasks(Tasks tasks)
        {
            if (!ModelState.IsValid)
            {
                ExceptionManager.Validation(10004, "Se deben enviar todos los atributos del modelo tarea");
            }

            using (TasksEFModel db = new TasksEFModel())
            {
                tasks.CreateTask = DateTime.Now;
                db.Tasks.Add(tasks);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (TasksExists(tasks.IdTask))
                    {
                        return Conflict();
                    }
                    else
                    {
                        ExceptionManager.ReportException(this, ex);
                    }
                }
            }
            return CreatedAtRoute("DefaultApi", new { id = tasks.IdTask }, tasks);
        }

        /// <summary>
        /// Metodo DELETE, recibe el taskid, y realiza la eliminacion de forma logica, ubicando el estado actual en que
        /// se encuentra para cambiarlo dependiendo la solicitud request
        /// </summary>
        /// <param name="id"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // DELETE: api/Tasks/5
        [ResponseType(typeof(Tasks))]
        public async Task<IHttpActionResult> DeleteTasks(int id)
        {
            if (id != 0)
            {
                try
                {
                    using (TasksEFModel db = new TasksEFModel())
                    {
                        Tasks tasks = await db.Tasks.FindAsync(id);
                        if (tasks == null)
                        {
                            ExceptionManager.Validation(10005, "Tarea no encontrada");
                        }

                        switch (tasks.Status)
                        {
                            case 0:
                                tasks.Status = 1;
                                break;
                            case 1:
                                tasks.Status = 0;
                                break;
                        }

                        await db.SaveChangesAsync();
                        return Ok(tasks);
                    }
                }
                catch (Exception ex)
                {
                    return ExceptionManager.ReportException(this, ex);
                }
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (TasksEFModel db = new TasksEFModel())
                {
                    db.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private bool TasksExists(int id)
        {
            try
            {
                using (TasksEFModel db = new TasksEFModel())
                {
                    return db.Tasks.Count(e => e.IdTask == id) > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}