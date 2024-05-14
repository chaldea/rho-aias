import { postUserChangePassword } from '@/services/dashboard/user';
import { defaultPageContainer } from '@/shared/page';
import { useStyles } from '@/shared/style';
import { PageContainer, ProCard, ProForm, ProFormText } from '@ant-design/pro-components';
import { useIntl } from '@umijs/max';
import { message } from 'antd';

const Account: React.FC = () => {
  const { styles } = useStyles();
  const intl = useIntl();
  return (
    <PageContainer {...defaultPageContainer} className={styles.container} style={{ padding: 15 }}>
      <ProCard title={intl.formatMessage({ id: 'pages.user.account.title' })} bordered>
        <ProForm<API.UserChangePasswordDto>
          onFinish={async (values: API.UserChangePasswordDto) => {
            try {
              const result = await postUserChangePassword(values);
              if (result.isSuccess) {
                message.success(intl.formatMessage({ id: 'pages.user.account.success' }));
                return true;
              }
              message.error(intl.formatMessage({ id: `pages.error.code.${result.code}` }));
              return false;
            } catch (error) {
              return false;
            }
          }}
          submitter={{
            searchConfig: {
              submitText: intl.formatMessage({ id: 'pages.user.account.submit' }),
            },
            resetButtonProps: false,
          }}
        >
          <ProFormText.Password
            name="oldPassword"
            label={intl.formatMessage({ id: 'pages.user.account.oldPassword' })}
            rules={[
              {
                required: true,
                message: intl.formatMessage({ id: 'pages.user.account.oldPassword.required' }),
              },
            ]}
            placeholder={intl.formatMessage({ id: 'pages.user.account.oldPassword.placeholder' })}
          />
          <ProFormText.Password
            name="newPassword"
            label={intl.formatMessage({ id: 'pages.user.account.newPassword' })}
            rules={[
              {
                required: true,
                message: intl.formatMessage({ id: 'pages.user.account.newPassword.required' }),
              },
            ]}
            placeholder={intl.formatMessage({ id: 'pages.user.account.newPassword.placeholder' })}
          />
          <ProFormText.Password
            name="confirmedPassword"
            label={intl.formatMessage({ id: 'pages.user.account.confirmedPassword' })}
            rules={[
              {
                required: true,
                message: intl.formatMessage({
                  id: 'pages.user.account.confirmedPassword.required',
                }),
              },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(
                    new Error(
                      intl.formatMessage({ id: 'pages.user.account.confirmedPassword.error' }),
                    ),
                  );
                },
              }),
            ]}
            placeholder={intl.formatMessage({
              id: 'pages.user.account.confirmedPassword.placeholder',
            })}
          />
        </ProForm>
      </ProCard>
    </PageContainer>
  );
};
export default Account;
