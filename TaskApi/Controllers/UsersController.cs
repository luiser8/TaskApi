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
        private TasksEFModel db = new TasksEFModel();
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
            Users users = await db.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            users.Password = Utils.MD5.GetMD5(users.Password); 
            return Ok(users);
        }

        // GET: api/Users?email
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> GetEmail(string email)
        {
            bool ok = false;
            try
            {
                var result = await db.Users.SingleAsync(u => u.Email == email);

                if (result == null)
                {
                    ok = false;
                }
                else
                {
                    ok = true;
                }
            }
            catch (Exception)
            {

            }
            return Ok(ok);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUsers(int id, Users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != users.IdUser)
            {
                return BadRequest();
            }

            db.Entry(users).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Users
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostUsers(Users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            users.CreateUser = DateTime.Now;
            users.Password = Utils.MD5.GetMD5(users.Password);
            db.Users.Add(users);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = users.IdUser }, users);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> DeleteUsers(int id)
        {
            Users users = await db.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            db.Users.Remove(users);
            await db.SaveChangesAsync();

            return Ok(users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsersExists(int id)
        {
            return db.Users.Count(e => e.IdUser == id) > 0;
        }
    }
}