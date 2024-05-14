// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';

/** 此处后端没有提供注释 POST /api/dashboard/user/change-password */
export async function postUserChangePassword(
  body: API.UserChangePasswordDto,
  options?: { [key: string]: any },
) {
  return request<API.Result>('/api/dashboard/user/change-password', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 POST /api/dashboard/user/login */
export async function postUserLogin(body: API.LoginDto, options?: { [key: string]: any }) {
  return request<API.LoginResultDto>('/api/dashboard/user/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

/** 此处后端没有提供注释 GET /api/dashboard/user/profile */
export async function getUserProfile(options?: { [key: string]: any }) {
  return request<API.UserProfileDto>('/api/dashboard/user/profile', {
    method: 'GET',
    ...(options || {}),
  });
}
