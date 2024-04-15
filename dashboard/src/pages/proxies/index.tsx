import ThemeSwitch from '@/components/ThemeSwitch';
import { getClientList } from '@/services/dashboard/client';
import { deleteProxyRemove, getProxyList, putProxyCreate } from '@/services/dashboard/proxy';
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
    target = `${proxyTypeEnum[record.type!].text}://${record.localIP}:${record.localPort}`;
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
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.ProxyDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '类型',
      dataIndex: 'type',
      valueEnum: proxyTypeEnum,
    },
    {
      title: '转发规则',
      render: (text, record, _, action) => forwardAddr(record),
    },
    {
      title: '客户端',
      render: (text, record, _, action) => record.client?.name,
    },
    {
      title: '操作',
      valueType: 'option',
      key: 'option',
      render: (text, record, _, action) => [
        <a key="edit">编辑</a>,
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
          删除
        </a>,
      ],
    },
  ];

  const getClients = async () => {
    const result = await getClientList();
    const data = result.map((x) => ({ value: x.id!, label: x.name! }));
    setClients(data);
  };

  useEffect(() => {
    getClients();
  }, []);

  return (
    <PageContainer fixedHeader header={{ title: '', breadcrumb: {} }} className={styles.container}>
      <ProTable<API.ProxyDto>
        rowKey="id"
        headerTitle="转发列表"
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
            新建
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
        title="创建转发规则"
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
        <ProFormText name="name" label="名称" />
        <ProFormSelect name="clientId" label="客户端" options={clients} />
        <ProFormSelect
          name="type"
          label="类型"
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
            label="端口"
            fieldProps={{
              moneySymbol: false,
            }}
            min={0}
            max={65535}
          />
        )}
        {[0, 1].includes(proxyType) && (
          <>
            <ProFormTextArea
              colProps={{ span: 24 }}
              name="hosts"
              label="域名"
              placeholder="请求域名，多个域名使用换行"
            />
            <ProFormText name="path" label="Path" initialValue="{**catch-all}" />
          </>
        )}
        <ProFormText name="localIP" label="目标IP" />
        <ProFormMoney
          name="localPort"
          label="目标端口"
          fieldProps={{
            moneySymbol: false,
          }}
          min={0}
          max={65535}
        />
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Forwards;
