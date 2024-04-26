import { history } from '@umijs/max';
import { stringify } from 'querystring';

export const loginOut = async () => {
  removeToken();
  const { search, pathname } = window.location;
  const urlParams = new URL(window.location.href).searchParams;
  /** 此方法会跳转到 redirect 参数所在的位置 */
  const redirect = urlParams.get('redirect');
  const path = pathname.replace('/dashboard', '');
  // Note: There may be security issues, please note
  if (window.location.pathname !== '/user/login' && !redirect) {
    history.replace({
      pathname: '/user/login',
      search: stringify({
        redirect: path + search,
      }),
    });
  }
};

export const getToken = (): string => {
  return window.sessionStorage.getItem('AccessToken') || '';
}

export const setToken = (token: string) => {
  window.sessionStorage.setItem('AccessToken', token);
}

export const removeToken = () => {
  window.sessionStorage.removeItem('AccessToken');
}
