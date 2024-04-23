﻿using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Alidns20150109.Models;
using Microsoft.Extensions.Logging;
using AliyunClient = AlibabaCloud.SDK.Alidns20150109.Client;

namespace Chaldea.Fate.RhoAias.Dns.Aliyun;

internal class AliyunDnsProvider : IDnsProvider
{
	private readonly ILogger<AliyunDnsProvider> _logger;

	public AliyunDnsProvider(ILogger<AliyunDnsProvider> logger)
	{
		_logger = logger;
	}

	public async Task<string?> CreateAsync(DnsProvider provider, string domain, string value)
	{
		try
		{
			var client = CreateClient(provider);
			var request = new AddDomainRecordRequest();
			request.DomainName = domain;
			request.RR = "_acme-challenge";
			request.Type = "TXT";
			request.Value = value;
			var response = await client.AddDomainRecordAsync(request);
			if (response.StatusCode == 200) return response.Body.RecordId;
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "");
			return null;
		}
	}

	public async Task<bool> RemoveAsync(DnsProvider provider, string id)
	{
		var client = CreateClient(provider);
		var request = new DeleteDomainRecordRequest
		{
			RecordId = id
		};
		var response = await client.DeleteDomainRecordAsync(request);
		if (response.StatusCode == 200) return true;
		return false;
	}

	private AliyunClient CreateClient(DnsProvider provider)
	{
		var config = provider.GetConfig<Config>();
		if (string.IsNullOrEmpty(config.Endpoint)) config.Endpoint = "alidns.cn-hangzhou.aliyuncs.com";
		return new AliyunClient(config);
	}
}