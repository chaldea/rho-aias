import {
  deleteClientRemove,
  getClientList,
  postClientUpdate,
  putClientCreate,
} from '@/services/dashboard/client';
import { defaultPageContainer } from '@/shared/page';
import { useStyles } from '@/shared/style';
import { PlusOutlined } from '@ant-design/icons';
import {
  ActionType,
  ModalForm,
  PageContainer,
  ProColumns,
  ProFormText,
  ProTable,
} from '@ant-design/pro-components';
import { useIntl } from '@umijs/max';
import { Button, Modal } from 'antd';
import { useRef, useState } from 'react';

const Clients: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [modal, contextHolder] = Modal.useModal();
  const [editItem, setEditItem] = useState<API.ClientCreateDto>();
  const actionRef = useRef<ActionType>();
  const { styles } = useStyles();
  const intl = useIntl();
  const columns: ProColumns<API.ClientDto>[] = [
    {
      dataIndex: 'index',
      valueType: 'indexBorder',
      width: 48,
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.name' }),
      dataIndex: 'name',
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.version' }),
      dataIndex: 'version',
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.ip' }),
      dataIndex: 'endpoint',
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.token' }),
      dataIndex: 'token',
      copyable: true,
      ellipsis: true,
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.status' }),
      dataIndex: 'status',
      valueEnum: {
        true: {
          text: intl.formatMessage({ id: 'pages.clients.status.true' }),
          status: 'Success',
        },
        false: {
          text: intl.formatMessage({ id: 'pages.clients.status.false' }),
          status: 'Default',
        },
      },
    },
    {
      title: intl.formatMessage({ id: 'pages.clients.operation' }),
      valueType: 'option',
      key: 'option',
      render: (text, record, _, action) => [
        <a
          key="edit"
          onClick={() => {
            setEditItem({ id: record.id, name: record.name });
            setOpen(true);
          }}
        >
          {intl.formatMessage({ id: 'pages.clients.operation.edit' })}
        </a>,
        <a
          key="delete"
          onClick={async () => {
            const confirmed = await modal.confirm({
              title: intl.formatMessage({ id: 'pages.clients.deleteTitle' }),
              content: intl.formatMessage({ id: 'pages.clients.deleteMessage' }),
            });

            if (confirmed) {
              await deleteClientRemove({ id: record.id });
              actionRef.current?.reload();
            }
          }}
        >
          {intl.formatMessage({ id: 'pages.clients.operation.delete' })}
        </a>,
      ],
    },
  ];

  return (
    <PageContainer {...defaultPageContainer} className={styles.container}>
      <ProTable<API.ClientDto>
        rowKey="id"
        headerTitle={intl.formatMessage({ id: 'pages.clients.headerTitle' })}
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
            {intl.formatMessage({ id: 'pages.clients.create' })}
          </Button>,
        ]}
        request={async (params) => {
          const data = await getClientList();
          return {
            data,
            success: true,
            total: data.length,
          };
        }}
      />

      <ModalForm<API.ClientCreateDto>
        title={intl.formatMessage({ id: 'pages.clients.createTitle' })}
        width={500}
        open={open}
        onOpenChange={(visible) => {
          setOpen(visible);
          if (!visible) {
            setEditItem(undefined);
          }
        }}
        onFinish={async (values: API.ClientCreateDto) => {
          try {
            if (editItem) {
              values.id = editItem.id;
              await postClientUpdate(values);
            } else {
              await putClientCreate(values);
            }
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
        initialValues={editItem}
        modalProps={{
          maskClosable: false,
          destroyOnClose: true,
        }}
      >
        <ProFormText
          name="name"
          label={intl.formatMessage({ id: 'pages.clients.name' })}
          rules={[
            {
              required: true,
              message: intl.formatMessage({ id: 'pages.clients.name.required' }),
            },
          ]}
          placeholder={intl.formatMessage({ id: 'pages.clients.name.placeholder' })}
        />
      </ModalForm>
      {contextHolder}
    </PageContainer>
  );
};

export default Clients;
