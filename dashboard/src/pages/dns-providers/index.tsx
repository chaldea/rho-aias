import {
  deleteDnsProviderRemove,
  getDnsProviderList,
  putDnsProviderCreate,
} from '@/services/dashboard/dnsProvider';
import { defaultPageContainer } from '@/shared/page';
import { useStyles } from '@/shared/style';
import { PlusOutlined } from '@ant-design/icons';
import {
  ActionType,
  ModalForm,
  PageContainer,
  ProColumns,
  ProFormSelect,
  ProFormText,
  ProFormTextArea,
  ProTable,
} from '@ant-design/pro-components';
import { Button, Modal } from 'antd';
import { useRef, useState } from 'react';

const DnsProviders: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [modal, contextHolder] = Modal.useModal();
  const { styles } = useStyles();
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.DnsProviderDto>[] = [
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
      title: '配置',
      dataIndex: 'config',
      ellipsis: true,
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
              await deleteDnsProviderRemove({ id: record.id });
              actionRef.current?.reload();
            }
          }}
        >
          删除
        </a>,
      ],
    },
  ];

  return (
    <PageContainer {...defaultPageContainer} className={styles.container}>
      <ProTable<API.DnsProviderDto>
        rowKey="id"
        headerTitle="DNS服务商列表"
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
          const data = await getDnsProviderList();
          return {
            data,
            success: true,
            total: data.length,
          };
        }}
      />

      <ModalForm<API.DnsProviderDto>
        title="创建DNS提供商"
        width={500}
        open={open}
        modalProps={{ maskClosable: false, destroyOnClose: true }}
        onOpenChange={(visible) => setOpen(visible)}
        onFinish={async (values: API.DnsProviderDto) => {
          try {
            values.name = values.provider;
            await putDnsProviderCreate(values);
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
      >
        <ProFormSelect
          name="provider"
          label="服务商"
          options={[{ value: 'Aliyun', label: '阿里云' }]}
        />
        <ProFormTextArea
          name="config"
          label="DNS接口配置"
          placeholder="配置内容JSON"
          fieldProps={{ style: { height: 200 } }}
        />
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};
export default DnsProviders;
