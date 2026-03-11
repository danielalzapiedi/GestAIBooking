using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestAI.Application.Login
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, DateTime Expires);
}
