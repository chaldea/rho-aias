declare namespace API {
  type ClientCreateDto = {
    name?: string;
  };

  type ClientDto = {
    id?: string;
    name?: string;
    version?: string;
    token?: string;
    endpoint?: string;
    connectionId?: string;
    status?: boolean;
  };

  type deleteClientRemoveParams = {
    id?: string;
  };

  type deleteProxyRemoveParams = {
    id?: string;
  };

  type LoginDto = {
    userName?: string;
    password?: string;
    type?: string;
  };

  type LoginResultDto = {
    status?: string;
    token?: string;
    type?: string;
  };

  type ProxyDto = {
    id?: string;
    name?: string;
    type?: ProxyType;
    localIP?: string;
    localPort?: number;
    remotePort?: number;
    path?: string;
    hosts?: string[];
    clientId?: string;
    client?: ClientDto;
  };

  type ProxyType = 0 | 1 | 2 | 3;

  type SummaryDto = {
    version?: string;
    bindPort?: number;
    httpPort?: number;
    httpsPort?: number;
    proxies?: number;
    certs?: number;
  };

  type UserProfileDto = {
    id?: string;
    userName?: string;
    avatar?: string;
  };
}
