using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TaskApi.Models;
using TaskApi.Utils;

namespace TaskApi.Controllers
{
    public class UsersController : ApiController
    {
        /// <summary>
        /// Devuelve una lista JSON de la tabla Users
        /// </summary>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // GET: api/Users
        public IHttpActionResult GetUsers()
        {
            try
            {
                using (TasksEFModel db = new TasksEFModel())
                {
                    return Ok(db.Users.ToList());
                }
            }
            catch (Exception ex)
            {
                return ExceptionManager.ReportException(this, ex);
            }
        }

        /// <summary>
        /// Devuelve una lista JSON de la tabla Users
        /// </summary>
        /// <param name="id"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // GET: api/Users/5
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> GetUsers(int id)
        {
            if (id != 0)
            {
                try
                {
                    using (TasksEFModel db = new TasksEFModel())
                    {
                        Users users = await db.Users.FindAsync(id);
                        if (users == null)
                        {
                            ExceptionManager.Validation(20000, "Usuario no encontrado");
                        }
                        return Ok(users);
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
        /// Metodo PUT, Hacemos actualizacion de la tabla Users
        /// </summary>
        /// <param name="id"></param>
        /// <param name="users"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUsers(int id, Users users)
        {
            if (!ModelState.IsValid)
            {
                ExceptionManager.Validation(20001, "Se deben enviar todos los atributos del modelo de la usuario");
            }

            if (id != users.IdUser)
            {
                ExceptionManager.Validation(20002, "EL id de usuario debe coincidir para realizar el update");
            }

            using (TasksEFModel db = new TasksEFModel())
            {
                users.CreateUser = DateTime.Now;
                db.Entry(users).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!UsersExists(id))
                    {
                        ExceptionManager.Validation(20003, "Error de Concurrencia en la usuario");
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
        /// Metodo POST, recibe el request del form con los datos para hacer login
        /// </summary>
        /// <param name="user"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // POST: api/Login
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostUsersLogin(Users user)
        {
            if (user.Email != null && user.Password != null)
            {
                try
                {
                    using (TasksEFModel db = new TasksEFModel())
                    {
                        
                        string pass = MD5.GetMD5(user.Password);
                        
                        var result = await db.Users.Where(u => u.Email == user.Email).
                                                    Where(u => u.Password == pass)
                                                            .FirstOrDefaultAsync();

                        if (result == null)
                        {
                            ExceptionManager.Validation(20004, "Usuario no encontrado");
                        }

                        if (result.Status == 0)
                        {
                            ExceptionManager.Validation(20005, "Usuario no disponible");
                        }
                        if (result.Status == 1)
                        {
                            return Ok(result);
                        }
                        return Ok(result);
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
        /// Metodo POST, recibe el request del form con los datos para crear un nuevo registro en la tabla Users
        /// </summary>
        /// <param name="users"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // POST: api/Users
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostUsers(Users users)
        {
            if (!ModelState.IsValid)
            {
                ExceptionManager.Validation(20006, "Se deben enviar todos los atributos del modelo usuario");
            }

            using (TasksEFModel db = new TasksEFModel())
            {
                users.CreateUser = DateTime.Now;
                db.Users.Add(users);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (UsersExists(users.IdUser))
                    {
                        return Conflict();
                    }
                    else
                    {
                        ExceptionManager.ReportException(this, ex);
                    }
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = users.IdUser }, users);
        }

        /// <summary>
        /// Metodo DELETE, recibe el userid, y realiza la eliminacion de forma logica, ubicando el estado actual en que
        /// se encuentra para cambiarlo dependiendo la solicitud request
        /// </summary>
        /// <param name="id"></param>
        /// <returns> 
        ///     Retorna un objeto JSON
        /// </returns>
        /// <response code="200">Retorno de los registros</response>
        /// <response code="400">Retorno de null si no hay registros</response>
        // DELETE: api/Users/5
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> DeleteUsers(int id)
        {
            if (id != 0)
            {
                try
                {
                    using (TasksEFModel db = new TasksEFModel())
                    {
                        Users users = await db.Users.FindAsync(id);
                        if (users == null)
                        {
                            ExceptionManager.Validation(20007, "Usuario no encontrado");
                        }

                        switch (users.Status)
                        {
                            case 0:
                                users.Status = 1;
                                break;
                            case 1:
                                users.Status = 0;
                                break;
                        }

                        db.Entry(users).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        return Ok(users);
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

        private bool UsersExists(int id)
        {
            try
            {
                using (TasksEFModel db = new TasksEFModel())
                {
                    return db.Users.Count(e => e.IdUser == id) > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}