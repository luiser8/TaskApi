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

namespace TaskApi.Controllers
{
    public class LoginController : ApiController
    {
        private TasksEFModel db = new TasksEFModel();

        // POST: api/Login
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostUsers([FromBody] Users user)
        {
            string pass = Utils.MD5.GetMD5(user.Password);
            Users users = await db.Users.Where(u => u.Email == user.Email).
                            Where(u => u.Password == pass)
                            .FirstOrDefaultAsync();

            if (users == null)
            {
                return Ok(users);
            }

            return Ok(users);
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