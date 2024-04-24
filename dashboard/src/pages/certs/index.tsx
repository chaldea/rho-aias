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
import { Button, Modal } from 'antd';
import { useEffect, useRef, useState } from 'react';

const Certs: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [certType, setCertType] = useState<number>(0);
  const [dnsProviders, setDnsProviders] = useState<{ value: string; label: string }[]>([]);
  const [modal, contextHolder] = Modal.useModal();
  const { styles } = useStyles();
  const actionRef = useRef<ActionType>();
  const columns: ProColumns<API.CertDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: '域名',
      dataIndex: 'domain',
    },
    {
      title: '颁发机构',
      dataIndex: 'issuer',
    },
    {
      title: '邮箱',
      dataIndex: 'email',
    },
    {
      title: '有效期',
      dataIndex: 'expires',
      valueType: 'dateTime',
    },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        0: {
          text: '颁发中',
          status: 'Default',
        },
        1: {
          text: '已生效',
          status: 'Success',
        },
        2: {
          text: '颁发失败',
          status: 'Error',
        },
      },
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
              content: '确定要删除该证书？删除后可能导致域名无法访问。',
            });

            if (confirmed) {
              await deleteCertRemove({ id: record.id });
              actionRef.current?.reload();
            }
          }}
        >
          删除
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
        headerTitle="证书列表"
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
            申请
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
        title="申请证书"
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
          label="证书类型"
          options={[
            { value: 0, label: '单域名' },
            { value: 1, label: '泛域名' },
          ]}
        />
        <ProFormText
          name="domain"
          label={certType == 0 ? '单域名(eg: test.sample.com)' : '泛域名(eg: *.sample.com)'}
        />
        <ProFormSelect
          name="issuer"
          label="颁发者"
          options={[{ value: 'LetsEncrypt', label: 'LetsEncrypt' }]}
        />
        <ProFormText name="email" label="邮箱" />
        {certType == 1 && (
          <>
            <ProFormSelect name="dnsProviderId" label="DNS提供商" options={dnsProviders} />
          </>
        )}
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Certs;
