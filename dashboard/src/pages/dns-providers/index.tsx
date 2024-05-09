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
  ProFormTextArea,
  ProTable,
} from '@ant-design/pro-components';
import { useIntl } from '@umijs/max';
import { Button, Modal } from 'antd';
import { useRef, useState } from 'react';

const DnsProviders: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [modal, contextHolder] = Modal.useModal();
  const { styles } = useStyles();
  const intl = useIntl();
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.DnsProviderDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: intl.formatMessage({ id: 'pages.dns-providers.name' }),
      dataIndex: 'name',
    },
    {
      title: intl.formatMessage({ id: 'pages.dns-providers.config' }),
      dataIndex: 'config',
      ellipsis: true,
    },
    {
      title: intl.formatMessage({ id: 'pages.dns-providers.operation' }),
      valueType: 'option',
      key: 'option',
      render: (text, record, _, action) => [
        <a key="edit">{intl.formatMessage({ id: 'pages.dns-providers.operation.edit' })}</a>,
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
          {intl.formatMessage({ id: 'pages.dns-providers.operation.delete' })}
        </a>,
      ],
    },
  ];

  return (
    <PageContainer {...defaultPageContainer} className={styles.container}>
      <ProTable<API.DnsProviderDto>
        rowKey="id"
        headerTitle={intl.formatMessage({ id: 'pages.dns-providers.headerTitle' })}
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
            {intl.formatMessage({ id: 'pages.dns-providers.create' })}
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
        title={intl.formatMessage({ id: 'pages.dns-providers.createTitle' })}
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
          label={intl.formatMessage({ id: 'pages.dns-providers.provider' })}
          options={[
            {
              value: 'Aliyun',
              label: intl.formatMessage({ id: 'pages.dns-providers.provider.aliyun' }),
            },
          ]}
        />
        <ProFormTextArea
          name="config"
          label={intl.formatMessage({ id: 'pages.dns-providers.config' })}
          placeholder={intl.formatMessage({ id: 'pages.dns-providers.config.placeholder' })}
          fieldProps={{ style: { height: 200 } }}
        />
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};
export default DnsProviders;
