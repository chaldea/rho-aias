// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 PUT /api/dashboard/dns-provider/create */
export async function putDnsProviderCreate(
  body: API.DnsProviderDto,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/dns-provider/create', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/dns-provider/list */
export async function getDnsProviderList(options?: { [key: string]: any }) {
  return request<API.DnsProviderDto[]>('/api/dashboard/dns-provider/list', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 DELETE /api/dashboard/dns-provider/remove */
export async function deleteDnsProviderRemove(
  // 叠加生成的Param类型 (非body参数swagger默认没有生成对象)
  params: API.deleteDnsProviderRemoveParams,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/dns-provider/remove', {
    method: 'DELETE',
    params: {
      ...params,
    },
    ...(options || {}),
  });
}
