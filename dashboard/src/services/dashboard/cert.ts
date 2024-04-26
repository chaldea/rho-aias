// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 PUT /api/dashboard/cert/create */
export async function putCertCreate(body: API.CertCreateDto, options?: { [key: string]: any }) {
  return request<any>('/api/dashboard/cert/create', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/cert/list */
export async function getCertList(options?: { [key: string]: any }) {
  return request<API.CertDto[]>('/api/dashboard/cert/list', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 DELETE /api/dashboard/cert/remove */
export async function deleteCertRemove(
  // 叠加生成的Param类型 (非body参数swagger默认没有生成对象)
  params: API.deleteCertRemoveParams,
  options?: { [key: string]: any },
) {
  return request<any>('/api/dashboard/cert/remove', {
    method: 'DELETE',
    params: {
      ...params,
    },
    ...(options || {}),
  });
}
