import { ProLayoutProps } from '@ant-design/pro-components';

/**
 * @name
 */
const Settings: ProLayoutProps & {
  pwa?: boolean;
  logo?: string;
} = {
  navTheme: 'light',
  // 拂晓蓝
  colorPrimary: '#0086C9',
  layout: 'side',
  contentWidth: 'Fluid',
  fixedHeader: false,
  fixSiderbar: true,
  footerRender: false,
  colorWeak: false,
  title: 'RHO-AIAS',
  pwa: true,
  logo: 'https://gw.alipayobjects.com/zos/rmsportal/KDpgvguMpGfqaHPjicRK.svg',
  iconfontUrl: '',
  token: {
    // 参见ts声明，demo 见文档，通过token 修改样式
    //https://procomponents.ant.design/components/layout#%E9%80%9A%E8%BF%87-token-%E4%BF%AE%E6%94%B9%E6%A0%B7%E5%BC%8F
    sider: {
      // colorMenuBackground: '#FFFFFF',
      colorMenuBackground: '#065986',
      colorBgCollapsedButton: '#FFFFFF',
      colorTextMenu: '#FFFFFF',
      colorTextMenuSelected: '#FFFFFF',
      colorTextMenuItemHover: '#FFFFFF',
      colorTextMenuTitle: '#FFFFFF',
    },
  },
};

export default Settings;
