import { deleteCertRemove, getCertList, putCertCreate } from '@/services/dashboard/cert';
import { getDnsProviderList } from '@/services/dashboard/dnsProvider';
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
  ProTable,
} from '@ant-design/pro-components';
import { useIntl } from '@umijs/max';
import { Button, Modal } from 'antd';
import { useEffect, useRef, useState } from 'react';

const Certs: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [certType, setCertType] = useState<number>(0);
  const [dnsProviders, setDnsProviders] = useState<{ value: string; label: string }[]>([]);
  const [modal, contextHolder] = Modal.useModal();
  const { styles } = useStyles();
  const intl = useIntl();
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.CertDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.domain' }),
      dataIndex: 'domain',
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.issuer' }),
      dataIndex: 'issuer',
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.email' }),
      dataIndex: 'email',
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.expires' }),
      dataIndex: 'expires',
      valueType: 'dateTime',
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.status' }),
      dataIndex: 'status',
      valueEnum: {
        0: {
          text: intl.formatMessage({ id: 'pages.certs.status.0' }),
          status: 'Default',
        },
        1: {
          text: intl.formatMessage({ id: 'pages.certs.status.1' }),
          status: 'Success',
        },
        2: {
          text: intl.formatMessage({ id: 'pages.certs.status.2' }),
          status: 'Error',
        },
      },
    },
    {
      title: intl.formatMessage({ id: 'pages.certs.operation' }),
      valueType: 'option',
      key: 'option',
      render: (text, record, _, action) => [
        <a key="edit">{intl.formatMessage({ id: 'pages.certs.operation.edit' })}</a>,
        <a
          key="delete"
          onClick={async () => {
            const confirmed = await modal.confirm({
              title: intl.formatMessage({ id: 'pages.certs.deleteTitle' }),
              content: intl.formatMessage({ id: 'pages.certs.deleteMessage' }),
            });

            if (confirmed) {
              await deleteCertRemove({ id: record.id });
              actionRef.current?.reload();
            }
          }}
        >
          {intl.formatMessage({ id: 'pages.certs.operation.delete' })}
        </a>,
      ],
    },
  ];

  const loadDnsProvider = async () => {
    const result = await getDnsProviderList();
    const data = result.map((x) => ({ value: x.id!, label: x.name! }));
    setDnsProviders(data);
  };

  useEffect(() => {
    loadDnsProvider();
  }, []);

  return (
    <PageContainer {...defaultPageContainer} className={styles.container}>
      <ProTable<API.CertDto>
        rowKey="id"
        headerTitle={intl.formatMessage({ id: 'pages.certs.headerTitle' })}
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
            {intl.formatMessage({ id: 'pages.certs.create' })}
          </Button>,
        ]}
        request={async (params) => {
          const data = await getCertList();
          return {
            data,
            success: true,
            total: data.length,
          };
        }}
      />

      <ModalForm<API.CertCreateDto>
        title={intl.formatMessage({ id: 'pages.certs.createTitle' })}
        width={500}
        open={open}
        modalProps={{ maskClosable: false, destroyOnClose: true }}
        onOpenChange={(visible) => setOpen(visible)}
        initialValues={{ certType }}
        onValuesChange={(changed) => {
          if (changed.certType !== undefined) setCertType(changed.certType);
        }}
        onFinish={async (values: API.CertCreateDto) => {
          try {
            await putCertCreate(values);
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
      >
        <ProFormSelect
          name="certType"
          label={intl.formatMessage({ id: 'pages.certs.certType' })}
          options={[
            { value: 0, label: intl.formatMessage({ id: 'pages.certs.certType.0' }) },
            { value: 1, label: intl.formatMessage({ id: 'pages.certs.certType.1' }) },
          ]}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.certs.certType.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.certs.certType.placeholder' })}
        />
        {certType == 1 && (
          <>
            <ProFormSelect
              name="dnsProviderId"
              label={intl.formatMessage({ id: 'pages.certs.dnsProvider' })}
              options={dnsProviders}
              rules={[
                {
                  required: true,
                  message: intl.formatMessage({ id: 'pages.certs.dnsProvider.required' }),
                },
              ]}
              placeholder={intl.formatMessage({ id: 'pages.certs.dnsProvider.placeholder' })}
            />
          </>
        )}
        <ProFormSelect
          name="issuer"
          label={intl.formatMessage({ id: 'pages.certs.issuer' })}
          options={[{ value: 'LetsEncrypt', label: 'LetsEncrypt' }]}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.certs.issuer.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.certs.issuer.placeholder' })}
        />
        <ProFormText
          name="domain"
          label={intl.formatMessage({ id: 'pages.certs.domain' })}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.certs.domain.required' }),
            },
          ]}
          placeholder={intl.formatMessage(
            { id: 'pages.certs.domain.placeholder' },
            { domain: certType == 0 ? 'test.sample.com' : '*.sample.com' },
          )}
        />
        <ProFormText
          name="email"
          label={intl.formatMessage({ id: 'pages.certs.email' })}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.certs.email.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.certs.email.placeholder' })}
        />
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Certs;
