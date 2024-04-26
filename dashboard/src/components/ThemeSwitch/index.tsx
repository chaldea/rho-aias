import { MoonOutlined, SunOutlined } from '@ant-design/icons';
import { useModel } from '@umijs/max';
import { Switch } from 'antd';
import { useState } from 'react';

const ThemeSwitch: React.FC = () => {
  const [isDark, setIsDark] = useState<boolean>(false);
  const { initialState, setInitialState } = useModel('@@initialState');

  const changeTheme = (checked: boolean) => {
    setIsDark(checked);
    const settings = { ...initialState?.settings } as any;
    if (checked) {
      settings.navTheme = 'realDark';
      settings.token = {};
    } else {
      settings.navTheme = 'light';
      settings.token = {
        sider: {
          colorMenuBackground: '#2E4051',
          colorBgCollapsedButton: '#FFFFFF',
          colorTextMenu: '#FFFFFF',
          colorTextMenuSelected: '#FFFFFF',
          colorTextMenuItemHover: '#FFFFFF',
          colorTextMenuTitle: '#FFFFFF',
        },
      };
    }
    setInitialState((preInitialState) => ({
      ...preInitialState,
      settings,
    }));
  };

  return (
    <Switch
      checked={isDark}
      onChange={changeTheme}
      unCheckedChildren={<MoonOutlined />}
      checkedChildren={<SunOutlined />}
    />
  );
};

export default ThemeSwitch;
