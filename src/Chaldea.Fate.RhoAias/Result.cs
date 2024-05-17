using System.ComponentModel;
using System.Reflection;

namespace Chaldea.Fate.RhoAias;

public struct Result
{
	public bool IsSuccess { get; set; }
	public int Code { get; set; }
	public string? Message { get; set; }

	public static Result Success()
	{
		return new Result
		{
			IsSuccess = true,
			Code = 0
		};
	}

	public static Result Error((int code, string? message) error)
	{
		return new Result
		{
			Code = error.code,
			Message = error.message
		};
	}
}

public struct Result<T> where T : class
{
	public bool IsSuccess { get; set; }
	public int Code { get; set; }
	public string? Message { get; set; }
	public T? Data { get; set; }

	public static Result<T> Success(T data)
	{
		return new Result<T>
		{
			IsSuccess = true,
			Code = 0,
			Data = data
		};
	}
}

/*
 * Module
 * 100-199: clients
 * 200-299: proxies
 * 300-399: certs
 * 400-499: dns
 * 500-599: user
 */
internal enum ErrorCode
{
	[Description("Invalid parameter: {0}.")]
	InvalidParameter = 1,

	[Description("Invalid client token, please update the token.")]
	InvalidClientToken = 100,

	[Description("Invalid client version, server version: {0}, client version: {1}.")]
	InvalidClientVersion = 102,

	[Description("Invalid client key {0}")]
	InvalidClientKey = 104
}

internal static class ErrorCodeExtensions
{
	private static readonly Dictionary<ErrorCode, string> Errors = new();

	static ErrorCodeExtensions()
	{
		var fields = typeof(ErrorCode).GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (var field in fields)
		{
			var code = (ErrorCode)field.GetValue(null);
			var attr = field.GetCustomAttribute<DescriptionAttribute>();
			Errors.TryAdd(code, attr.Description);
		}
	}

	public static (int, string) ToError(this ErrorCode errorCode, params object[] args)
	{
		if (Errors.TryGetValue(errorCode, out var msg)) return ((int)errorCode, string.Format(msg, args));

		return ((int)errorCode, string.Empty);
	}
}