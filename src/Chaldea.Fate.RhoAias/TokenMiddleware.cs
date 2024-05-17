using Microsoft.AspNetCore.Http;

namespace Chaldea.Fate.RhoAias
{
	internal class TokenMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IClientManager _clientManager;
		private readonly ITokenManager _tokenManager;

		public TokenMiddleware(
			RequestDelegate next, 
			IClientManager clientManager, 
			ITokenManager tokenManager)
		{
			_next = next;
			_clientManager = clientManager;
			_tokenManager = tokenManager;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Method != "TOKEN")
			{
				await _next(context);
				return;
			}

			var key = context.Request.Query["token_key"];
			if (string.IsNullOrEmpty(key))
			{
				var result = Result.Error(ErrorCode.InvalidParameter.ToError("token_key"));
				context.Response.StatusCode = 400;
				await context.Response.WriteAsJsonAsync(result);
				return;
			}

			var client = await _clientManager.GetClientAsync(key!);
			if (client == null)
			{
				var result = Result.Error(ErrorCode.InvalidClientKey.ToError(key!));
				context.Response.StatusCode = 400;
				await context.Response.WriteAsJsonAsync(result);
				return;
			}

			var token = await _tokenManager.CreateAsync(client.Id, Role.Client, 30 * 86400);
			await context.Response.WriteAsJsonAsync(token);
		}
	}
}
