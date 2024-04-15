import { createStyles } from "antd-style";

export const useStyles = createStyles((token) => {
  return {
    container: {
      ['& .ant-page-header']: {
        // borderBottom: `1px solid ${token.token.colorBorderSecondary}`,
        // paddingBlockStart: 0,
        // paddingBlockEnd: 0,
        // paddingInlineStart: 20,
        // paddingInlineEnd: 20,
        // height: 54,
      },

      ['& .ant-pro-grid-content .ant-pro-grid-content-children .ant-pro-page-container-children-container']:
      {
        padding: 0,
      },
    },
  };
});
