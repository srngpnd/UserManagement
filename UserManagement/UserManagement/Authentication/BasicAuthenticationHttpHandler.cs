using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using UserManagement.Service;

namespace UserManagement.Authentication
{
    public class BasicAuthenticationHttpHandler : IHttpModule
    {
        private const string Realm = "User Management";

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private static bool CheckPassword(string username, string password)
        {
            var userService = new UserService();
            return userService.AuthenticateUser(username, password);
        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string email = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (CheckPassword(email, password))
                {
                    var identity = new GenericIdentity(email);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                HttpContext.Current.Response.StatusCode = 401;
            }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
               AuthenticateUser(authHeader);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.End();
            }
        }

        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate",
                    string.Format("Basic realm=\"{0}\"", Realm));
            }
        }

        public void Dispose()
        {
        }
    }
}