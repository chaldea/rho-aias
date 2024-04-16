import { PageContainer, StatisticCard } from '@ant-design/pro-components';
import { Card, Col, Descriptions, Progress, Row } from 'antd';
import { useStyles } from '@/shared/style';
import { CloudServerOutlined, DatabaseOutlined } from '@ant-design/icons';
import { defaultPageContainer } from '@/shared/page';

const topColResponsiveProps = {
  xs: 24,
  sm: 12,
  md: 12,
  lg: 12,
  xl: 6,
  style: {
    marginBottom: 24,
  },
};

const Overview: React.FC = () => {
  const { styles } = useStyles();
  return (
    <PageContainer {...defaultPageContainer} className={styles.container} style={{ padding: 24 }}>
      <Row gutter={24}>
        <Col {...topColResponsiveProps}>
          <StatisticCard
            statistic={{
              title: <h3>客户端总数</h3>,
              value: 1000,
              icon: <DatabaseOutlined style={{ fontSize: 40, color: '#66AFF4' }} />,
            }}
          />
        </Col>
        <Col {...topColResponsiveProps}>
          <StatisticCard
            statistic={{
              title: <h3>在线数量</h3>,
              value: 1000,
              icon: <CloudServerOutlined style={{ fontSize: 40, color: '#C7A9F0' }} />,
            }}
          />
        </Col>
        <Col {...topColResponsiveProps}>
          <StatisticCard
            statistic={{
              title: <h3>流入流量</h3>,
              value: 3701928,
            }}
            chart={
              <img
                src="https://gw.alipayobjects.com/zos/alicdn/ShNDpDTik/huan.svg"
                alt="百分比"
                width="100%"
              />
            }
            chartPlacement="left"
          />
        </Col>
        <Col {...topColResponsiveProps}>
          <StatisticCard
            statistic={{
              title: <h3>流出流量</h3>,
              value: 3701928,
            }}
            chart={
              <img
                src="https://gw.alipayobjects.com/zos/alicdn/6YR18tCxJ/huanlv.svg"
                alt="百分比"
                width="100%"
              />
            }
            chartPlacement="left"
          />
        </Col>
      </Row>

      <Row gutter={24}>
        <Col xl={12} lg={24} md={24} sm={24} xs={24}>
          <Descriptions layout="horizontal" bordered>
            <Descriptions.Item label="Version" span={3}>
              1.0.0.1
            </Descriptions.Item>
            <Descriptions.Item label="BindPort" span={3}>
              8024
            </Descriptions.Item>
            <Descriptions.Item label="HttpPort" span={3}>
              80
            </Descriptions.Item>
            <Descriptions.Item label="HttpsPort" span={3}>
              443
            </Descriptions.Item>
            <Descriptions.Item label="Proxies" span={3}>
              10
            </Descriptions.Item>
            <Descriptions.Item label="Certs" span={3}>
              5
            </Descriptions.Item>
          </Descriptions>
        </Col>
        <Col xl={12} lg={24} md={24} sm={24} xs={24}>
          <Card bodyStyle={{ padding: 13 }}>
            <Descriptions layout="vertical">
              <Descriptions.Item label="cpu" span={3} style={{ paddingBottom: 0 }}>
                <Progress percent={30} status="normal" strokeWidth={6} strokeColor="#52C41A" />
              </Descriptions.Item>
              <Descriptions.Item label="memory" span={3} style={{ paddingBottom: 0 }}>
                <Progress percent={85} status="normal" strokeWidth={6} strokeColor="#FF4D4F" />
              </Descriptions.Item>
            </Descriptions>
            <Descriptions layout="horizontal">
              <Descriptions.Item label="tcp" span={3}>
                100
              </Descriptions.Item>
              <Descriptions.Item label="udp" span={3}>
                100
              </Descriptions.Item>
              <Descriptions.Item label="in" span={3}>
                5kb/s
              </Descriptions.Item>
              <Descriptions.Item label="out" span={3}>
                10kb/s
              </Descriptions.Item>
              <Descriptions.Item label="qps" span={3}>
                11kb/s
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>
      </Row>
    </PageContainer>
  );
};

export default Overview;
