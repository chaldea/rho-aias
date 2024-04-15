// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 PUT /api/dashboard/client/create */
export async function putClientCreate(body: API.ClientCreateDto, options?: { [key: string]: any }) {
  return request<any>('/api/dashboard/client/create', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/client/list */
export async function getClientList(options?: { [key: string]: any }) {
  return request<API.ClientDto[]>('/api/dashboard/client/list', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 DELETE /api/dashboard/client/remove */
export async function deleteClientRemove(
  // 叠加生成的Param类型 (非body参数swagger默认没有生成对象)
  params: API.deleteClientRemoveParams,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/client/remove', {
    method: 'DELETE',
    params: {
      ...params,
    },
    ...(options || {}),
  });
}
