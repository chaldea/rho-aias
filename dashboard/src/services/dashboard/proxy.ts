// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 PUT /api/dashboard/proxy/create */
export async function putProxyCreate(body: API.ProxyDto, options?: { [key: string]: any }) {
  return request<any>('/api/dashboard/proxy/create', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/proxy/list */
export async function getProxyList(options?: { [key: string]: any }) {
  return request<API.ProxyDto[]>('/api/dashboard/proxy/list', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 DELETE /api/dashboard/proxy/remove */
export async function deleteProxyRemove(
  // 叠加生成的Param类型 (非body参数swagger默认没有生成对象)
  params: API.deleteProxyRemoveParams,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/proxy/remove', {
    method: 'DELETE',
    params: {
      ...params,
    },
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 POST /api/dashboard/proxy/update */
export async function postProxyUpdate(body: API.ProxyDto, options?: { [key: string]: any }) {
  return request<any>('/api/dashboard/proxy/update', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 POST /api/dashboard/proxy/update-status */
export async function postProxyUpdateStatus(
  body: API.ProxyStatusDto,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/proxy/update-status', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}
