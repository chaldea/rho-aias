// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 GET /api/dashboard/statistic/metrics */
export async function getStatisticMetrics(options?: { [key: string]: any }) {
  return request<any>('/api/dashboard/statistic/metrics', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/statistic/summary */
export async function getStatisticSummary(options?: { [key: string]: any }) {
  return request<API.SummaryDto>('/api/dashboard/statistic/summary', {
    method: 'GET',
    ...(options || {}),
  });
}
