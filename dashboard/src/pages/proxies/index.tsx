import { getClientList } from '@/services/dashboard/client';
import { deleteProxyRemove, getProxyList, putProxyCreate } from '@/services/dashboard/proxy';
import { defaultPageContainer } from '@/shared/page';
import { useStyles } from '@/shared/style';
import { PlusOutlined } from '@ant-design/icons';
import {
  ActionType,
  ModalForm,
  PageContainer,
  ProColumns,
  ProFormMoney,
  ProFormSelect,
  ProFormText,
  ProFormTextArea,
  ProTable,
} from '@ant-design/pro-components';
import { useIntl } from '@umijs/max';
import { Button, Flex, Modal, Popconfirm, Space } from 'antd';
import { useEffect, useRef, useState } from 'react';

const proxyTypeEnum: any = {
  0: { text: 'http' },
  1: { text: 'https' },
  2: { text: 'tcp' },
  3: { text: 'udp' },
};

const forwardAddr = (record: API.ProxyDto) => {
  let hosts: string[] = [];
  let target = '';
  if ([0, 1].includes(record.type!)) {
    hosts = record.hosts!.map((x) => `${proxyTypeEnum[record.type!].text}://${x}`);
    target = record.destination || '';
  } else {
    hosts = [`:${record.remotePort}`];
    target = `${record.localIP}:${record.localPort}`;
  }

  return (
    <Flex align="center">
      <Space direction="vertical" size={0}>
        {hosts.map((x) => x)}
      </Space>
      <div style={{ width: 30, textAlign: 'center' }}>{'->'}</div>
      <div>{target}</div>
    </Flex>
  );
};

const Forwards: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [proxyType, setProxyType] = useState<number>(0);
  const [clients, setClients] = useState<{ value: string; label: string }[]>([]);
  const [modal, contextHolder] = Modal.useModal();
  const { styles } = useStyles();
  const intl = useIntl();
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.ProxyDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: intl.formatMessage({ id: 'pages.proxies.name' }),
      dataIndex: 'name',
    },
    {
      title: intl.formatMessage({ id: 'pages.proxies.type' }),
      dataIndex: 'type',
      valueEnum: proxyTypeEnum,
    },
    {
      title: intl.formatMessage({ id: 'pages.proxies.forward' }),
      render: (text, record, _, action) => forwardAddr(record),
    },
    {
      title: intl.formatMessage({ id: 'pages.proxies.client' }),
      render: (text, record, _, action) => record.client?.name,
    },
    {
      title: intl.formatMessage({ id: 'pages.proxies.operation' }),
      valueType: 'option',
      key: 'option',
      render: (text, record, _, action) => [
        <a key="edit">{intl.formatMessage({ id: 'pages.proxies.operation.edit' })}</a>,
        <a
          key="delete"
          onClick={async () => {
            const confirmed = await modal.confirm({
              title: '系统提示',
              content: '确定要删除该转发配置',
            });

            if (confirmed) {
              await deleteProxyRemove({ id: record.id });
              actionRef.current?.reload();
            }
          }}
        >
          {intl.formatMessage({ id: 'pages.proxies.operation.delete' })}
        </a>,
      ],
    },
  ];

  const getClients = async () => {
    const result = await getClientList();
    const data = result.map((x: any) => ({ value: x.id!, label: x.name! }));
    setClients(data);
  };

  useEffect(() => {
    getClients();
  }, []);

  return (
    <PageContainer {...defaultPageContainer} className={styles.container}>
      <ProTable<API.ProxyDto>
        rowKey="id"
        headerTitle={intl.formatMessage({ id: 'pages.proxies.headerTitle' })}
        actionRef={actionRef}
        search={false}
        columns={columns}
        toolBarRender={() => [
          <Button
            key="button"
            icon={<PlusOutlined />}
            type="primary"
            onClick={() => {
              setOpen(true);
            }}
          >
            {intl.formatMessage({ id: 'pages.proxies.create' })}
          </Button>,
        ]}
        request={async (params) => {
          const data = await getProxyList();
          return {
            data,
            success: true,
            total: data.length,
          };
        }}
      />

      <ModalForm<API.ProxyDto>
        title={intl.formatMessage({ id: 'pages.proxies.createTitle' })}
        width={500}
        open={open}
        modalProps={{ maskClosable: false, destroyOnClose: true }}
        onOpenChange={(visible) => setOpen(visible)}
        onValuesChange={(changed) => {
          if (changed.type !== undefined) setProxyType(changed.type);
        }}
        onFinish={async (values: API.ProxyDto) => {
          try {
            if (values.hosts) {
              values.hosts = (values.hosts as any).split(/\r?\n/);
            }
            await putProxyCreate(values);
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
      >
        <ProFormText name="name" label={intl.formatMessage({ id: 'pages.proxies.name' })} />
        <ProFormSelect
          name="clientId"
          label={intl.formatMessage({ id: 'pages.proxies.client' })}
          options={clients}
        />
        <ProFormSelect
          name="type"
          label={intl.formatMessage({ id: 'pages.proxies.type' })}
          options={[
            { value: 0, label: 'HTTP' },
            { value: 1, label: 'HTTPS' },
            { value: 2, label: 'TCP' },
            { value: 3, label: 'UDP' },
          ]}
        />
        {[2, 3].includes(proxyType) && (
          <ProFormMoney
            name="remotePort"
            label={intl.formatMessage({ id: 'pages.proxies.remotePort' })}
            fieldProps={{
              moneySymbol: false,
            }}
            min={0}
            max={65535}
          />
        )}
        {[0, 1].includes(proxyType) ? (
          <>
            <ProFormTextArea
              colProps={{ span: 24 }}
              name="hosts"
              label={intl.formatMessage({ id: 'pages.proxies.hosts' })}
              placeholder={intl.formatMessage({ id: 'pages.proxies.hosts.placeholder' })}
            />
            <ProFormText name="path" label="Path" initialValue="/{**catch-all}" />
            <ProFormText
              name="destination"
              label={intl.formatMessage({ id: 'pages.proxies.destination' })}
              placeholder="http://127.0.0.1:123/api"
            />
          </>
        ) : (
          <>
            <ProFormText
              name="localIP"
              label={intl.formatMessage({ id: 'pages.proxies.localIP' })}
            />
            <ProFormMoney
              name="localPort"
              label={intl.formatMessage({ id: 'pages.proxies.localPort' })}
              fieldProps={{
                moneySymbol: false,
              }}
              min={0}
              max={65535}
            />
          </>
        )}
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Forwards;
