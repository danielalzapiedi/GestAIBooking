using GestAI.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GestAI.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUser(IHttpContextAccessor http) => _http = http;

        public string UserId =>
            _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Usuario no autenticado.");

        public string? Email => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

        public string? FullName =>
            _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
            ?? _http.HttpContext?.User?.FindFirstValue("name");
    }
}
