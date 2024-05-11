import { getClientList } from '@/services/dashboard/client';
import {
  deleteProxyRemove,
  getProxyList,
  postProxyUpdate,
  putProxyCreate,
} from '@/services/dashboard/proxy';
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
import { Button, Flex, Modal, Popconfirm, Space, Typography } from 'antd';
import { useEffect, useRef, useState } from 'react';

const proxyTypeEnum: any = {
  0: { text: 'http' },
  1: { text: 'http' },
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
    hosts = [`0.0.0.0:${record.remotePort}`];
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
  const [editItem, setEditItem] = useState<API.ProxyDto>();
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
        <a
          key="edit"
          onClick={() => {
            const edit = { ...record };
            if (edit.hosts) {
              edit.hosts = edit.hosts.join('\n') as any;
            }
            setEditItem(edit);
            setOpen(true);
          }}
        >
          {intl.formatMessage({ id: 'pages.proxies.operation.edit' })}
        </a>,
        <a
          key="delete"
          onClick={async () => {
            const confirmed = await modal.confirm({
              title: intl.formatMessage({ id: 'pages.proxies.deleteTitle' }),
              content: intl.formatMessage({ id: 'pages.proxies.deleteMessage' }),
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
        modalProps={{
          maskClosable: false,
          destroyOnClose: true,
        }}
        initialValues={editItem || { path: '/{**catch-all}' }}
        onOpenChange={(visible) => {
          setOpen(visible);
          if (!visible) {
            setEditItem(undefined);
          }
        }}
        onValuesChange={(changed) => {
          if (changed.type !== undefined) setProxyType(changed.type);
        }}
        onFinish={async (values: API.ProxyDto) => {
          try {
            if (values.hosts) {
              values.hosts = (values.hosts as any).split(/\r?\n/);
            }
            if (editItem) {
              values.id = editItem.id;
              await postProxyUpdate(values);
            } else {
              await putProxyCreate(values);
            }
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
      >
        <ProFormText
          name="name"
          label={intl.formatMessage({ id: 'pages.proxies.name' })}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.proxies.name.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.proxies.name.placeholder' })}
        />
        <ProFormSelect
          name="clientId"
          label={intl.formatMessage({ id: 'pages.proxies.client' })}
          options={clients}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.proxies.client.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.proxies.client.placeholder' })}
        />
        <ProFormSelect
          name="type"
          label={intl.formatMessage({ id: 'pages.proxies.type' })}
          options={[
            { value: 0, label: 'HTTP' },
            { value: 2, label: 'TCP' },
            { value: 3, label: 'UDP' },
          ]}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.proxies.type.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.proxies.type.placeholder' })}
        />
        {[0, 1].includes(proxyType) ? (
          <>
            <ProFormTextArea
              colProps={{ span: 24 }}
              name="hosts"
              label={intl.formatMessage({ id: 'pages.proxies.hosts' })}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.hosts.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.proxies.hosts.placeholder' })}
              tooltip={intl.formatMessage({ id: 'pages.proxies.hosts.tooltip' })}
            />
            <ProFormText
              name="path"
              label="Path"
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.path.required' }),
                },
              ]}
              placeholder={intl.formatMessage(
                { id: 'pages.proxies.path.placeholder' },
                { path: `/{**catch-all}` },
              )}
            />
            <ProFormText
              name="destination"
              label={intl.formatMessage({ id: 'pages.proxies.destination' })}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.destination.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.proxies.destination.placeholder' })}
            />
          </>
        ) : (
          <>
            <ProFormMoney
              name="remotePort"
              label={intl.formatMessage({ id: 'pages.proxies.remotePort' })}
              fieldProps={{
                moneySymbol: false,
              }}
              min={0}
              max={65535}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.remotePort.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.proxies.remotePort.placeholder' })}
              tooltip={intl.formatMessage({ id: 'pages.proxies.remotePort.tooltip' })}
            />
            <ProFormText
              name="localIP"
              label={intl.formatMessage({ id: 'pages.proxies.localIP' })}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.localIP.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.proxies.localIP.placeholder' })}
            />
            <ProFormMoney
              name="localPort"
              label={intl.formatMessage({ id: 'pages.proxies.localPort' })}
              fieldProps={{
                moneySymbol: false,
              }}
              min={0}
              max={65535}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.proxies.localPort.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.proxies.localPort.placeholder' })}
            />
          </>
        )}
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Forwards;
