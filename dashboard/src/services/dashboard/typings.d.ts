declare namespace API {
  type AcmeStatus = 0 | 1 | 2;

  type CertCreateDto = {
    certType?: number;
    domain?: string;
    expires?: string;
    issuer?: string;
    email?: string;
    dnsProviderId?: string;
  };

  type CertDto = {
    id?: string;
    certType?: number;
    domain?: string;
    expires?: string;
    issuer?: string;
    email?: string;
    status?: AcmeStatus;
  };

  type ClientCreateDto = {
    id?: string;
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

  type deleteCertRemoveParams = {
    id?: string;
  };

  type deleteClientRemoveParams = {
    id?: string;
  };

  type deleteDnsProviderRemoveParams = {
    id?: string;
  };

  type deleteProxyRemoveParams = {
    id?: string;
  };

  type DnsProviderDto = {
    id?: string;
    name?: string;
    provider?: string;
    config?: string;
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
    destination?: string;
    clientId?: string;
    client?: ClientDto;
  };

  type ProxyType = 1 | 2 | 3 | 4;

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
