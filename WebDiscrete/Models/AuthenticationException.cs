using System;
using System.Linq.Expressions;

namespace WebDiscrete.Models
{
    public class AuthenticationException : Exception
    {
        private string message;

        public AuthenticationException(string message)
        {
            this.message = message;
        }
    }
}