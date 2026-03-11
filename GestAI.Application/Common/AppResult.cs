using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestAI.Application.Common
{
    public sealed record AppResult<T>(bool Success, T? Data, string? ErrorCode, string? Message)
    {
        public static AppResult<T> Ok(T data) => new(true, data, null, null);
        public static AppResult<T> Fail(string code, string message) => new(false, default, code, message);
    }

    public sealed record AppResult(bool Success, string? ErrorCode, string? Message)
    {
        public static AppResult Ok() => new(true, null, null);
        public static AppResult Fail(string code, string message) => new(false, code, message);
    }
}
