import { defaultPageContainer } from '@/shared/page';
import { useStyles } from '@/shared/style';
import { PageContainer } from '@ant-design/pro-components';
import { Card } from 'antd';

const Certs: React.FC = () => {
  const { styles } = useStyles();

  return <PageContainer {...defaultPageContainer} className={styles.container}></PageContainer>;
};

export default Certs;
