using System.Net;
using Microsoft.AspNetCore.Components;

namespace GestAI.Web;

public sealed class Redirect401Handler : DelegatingHandler
{
    private readonly NavigationManager _nav;
    private readonly ITokenStore _tokens;

    public Redirect401Handler(NavigationManager nav, ITokenStore tokens)
    {
        _nav = nav;
        _tokens = tokens;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var res = await base.SendAsync(request, cancellationToken);

        if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokens.ClearAsync();

            var path = new Uri(_nav.Uri).AbsolutePath;
            if (!path.Equals("/login", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = Uri.EscapeDataString(_nav.Uri);
                _nav.NavigateTo($"/login?returnUrl={returnUrl}", forceLoad: true);
            }
        }

        return res;
    }
}
