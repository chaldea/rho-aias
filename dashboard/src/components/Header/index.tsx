import { Flex } from 'antd';
import { createStyles } from 'antd-style';
import ThemeSwitch from '../ThemeSwitch';

export const useStyles = createStyles((token) => {
  return {
    header: {
      height: 55,
      borderBottom: `1px solid ${token.token.colorBorderSecondary}`,
      padding: '0px 30px',
    },
  };
});

const Header: React.FC = () => {
  const { styles } = useStyles();

  return (
    <Flex justify="flex-end" align="center" className={styles.header}>
      <ThemeSwitch />
    </Flex>
  );
};

export default Header;
