import { createStyles } from 'antd-style';
import ThemeSwitch from '../ThemeSwitch';

export const useStyles = createStyles((token) => {
  let bgColor = token.token.colorBgElevated;
  if (token.token.colorBgElevated == '#1f1f1f') {
    bgColor = '#131313';
  }
  return {
    headerWrap: {
      height: 55,
      borderBottom: `1px solid ${token.token.colorBorderSecondary}`,
      position: 'fixed',
      zIndex: 10,
      backgroundColor: `${bgColor}`,
      width: '100%',
    },
    header: {
      zIndex: 11,
      position: 'fixed',
      top: 15,
      right: 15,
    },
  };
});

const Header: React.FC = () => {
  const { styles } = useStyles();
  return (
    <>
      <div className={styles.header}>
        <ThemeSwitch />
      </div>
      <div className={styles.headerWrap}></div>
    </>
  );
};

export default Header;
