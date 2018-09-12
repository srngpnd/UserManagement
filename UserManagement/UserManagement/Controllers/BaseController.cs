using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using UserManagement.Context;

namespace UserManagement.Controllers
{
    public class BaseController : ApiController
    {
        protected DataContext _dbContext = new DataContext();
    }
}