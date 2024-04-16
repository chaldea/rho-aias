import { deleteClientRemove, getClientList, putClientCreate } from '@/services/dashboard/client';
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
import { Button, Modal } from 'antd';
import { useRef, useState } from 'react';

const Clients: React.FC = () => {
  const [open, setOpen] = useState<boolean>(false);
  const [modal, contextHolder] = Modal.useModal();
  const actionRef = useRef<ActionType>();
  const { styles } = useStyles();
  const columns: ProColumns<API.ClientDto>[] = [
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
      title: '版本',
      dataIndex: 'version',
    },
    {
      title: 'IP',
      dataIndex: 'endpoint',
    },
    {
      title: 'Token',
      dataIndex: 'token',
      copyable: true,
      ellipsis: true,
    },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        true: {
          text: '在线',
          status: 'Success',
        },
        false: {
          text: '离线',
          status: 'Default',
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
              content: '确定要删除该客户端及其转发配置？',
            });

            if (confirmed) {
              await deleteClientRemove({ id: record.id });
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
      <ProTable<API.ClientDto>
        rowKey="id"
        headerTitle="客户端列表"
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
          const data = await getClientList();
          return {
            data,
            success: true,
            total: data.length,
          };
        }}
      />

      <ModalForm<API.ClientCreateDto>
        title="创建客户端"
        width={500}
        open={open}
        onOpenChange={(visible) => setOpen(visible)}
        onFinish={async (values: API.ClientCreateDto) => {
          try {
            await putClientCreate(values);
            actionRef.current?.reload();
            return true;
          } catch (error) {
            return false;
          }
        }}
      >
        <ProFormText name="name" label="名称" />
      </ModalForm>

      {contextHolder}
    </PageContainer>
  );
};

export default Clients;
