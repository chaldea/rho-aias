import { PageContainer, StatisticCard } from '@ant-design/pro-components';
import { Card, Col, Descriptions, Progress, Row } from 'antd';
import { useStyles } from '@/shared/style';
import { CloudServerOutlined, DatabaseOutlined } from '@ant-design/icons';
import { defaultPageContainer } from '@/shared/page';
import { getStatisticMetrics, getStatisticSummary } from '@/services/dashboard/statistic';
import { useEffect, useState } from 'react';
import { createSignalRContext } from 'react-signalr/signalr';
import * as signalR from '@microsoft/signalr';
import { Tiny } from '@ant-design/plots';

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

type Metric = {
  client_all: number;
  client_online: number;
  traffic_in_sec: number;
  traffic_out_sec: number;
  traffic_total_sec: number;
  traffic_in_total: number;
  traffic_out_total: number;
  connection_tcp: number;
  connection_udp: number;
  system_cpu: number;
  system_memory: number;
};

const token = window.sessionStorage.getItem('AccessToken');
const SignalRContext = createSignalRContext();

function formatBytes(bytes: number, decimals: number = 2) {
  if (!+bytes) return '0 Bytes';

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
}

const getPrecent = (used: number, total: number) => {
  const p = used / total;
  return p === 0 ? 0.0001 : +p.toFixed(4);
};

const getConfig = (percent: number, color: string) => {
  return {
    percent: percent,
    width: 65,
    height: 65,
    color: ['#E8EFF5', color],
    annotations: [
      {
        type: 'text',
        style: {
          text: `${~~(percent * 100)}%`,
          x: '50%',
          y: '50%',
          textAlign: 'center',
          fontSize: 12,
        },
      },
    ],
  };
};

const Overview: React.FC = () => {
  const { styles } = useStyles();
  const [metrics, setMetrics] = useState<Metric>({
    client_all: 0,
    client_online: 0,
    traffic_in_sec: 0,
    traffic_out_sec: 0,
    traffic_total_sec: 0,
    traffic_in_total: 0,
    traffic_out_total: 0,
    connection_tcp: 0,
    connection_udp: 0,
    system_cpu: 0,
    system_memory: 0,
  });
  const [summary, setSummary] = useState<API.SummaryDto>();

  const [totalIn, setTotalIn] = useState<number>(0);
  const [totalOut, setTotalOut] = useState<number>(0);

  const updateMetrics = (args: Metric) => {
    setMetrics(args);
    const inP = getPrecent(args.traffic_in_total, args.traffic_in_total + args.traffic_out_total);
    const outP = getPrecent(args.traffic_out_total, args.traffic_in_total + args.traffic_out_total);
    if (inP !== totalIn) {
      setTotalIn(inP);
    }
    if (outP !== totalOut) {
      setTotalOut(outP);
    }
  };

  const loadMetrics = async () => {
    const data = await getStatisticMetrics();
    updateMetrics(data);
  };

  const loadSummary = async () => {
    const data = await getStatisticSummary();
    setSummary(data);
  };

  SignalRContext.useSignalREffect(
    'metrics',
    (args: Metric) => {
      updateMetrics(args);
    },
    [],
  );

  useEffect(() => {
    loadMetrics();
    loadSummary();
  }, []);

  return (
    <SignalRContext.Provider
      skipNegotiation={true}
      transport={signalR.HttpTransportType.WebSockets}
      connectEnabled={!!token}
      accessTokenFactory={() => token!}
      dependencies={[token]}
      url={`${window.location.origin}/notificationhub`}
    >
      <PageContainer {...defaultPageContainer} className={styles.container} style={{ padding: 24 }}>
        <Row gutter={24}>
          <Col {...topColResponsiveProps}>
            <StatisticCard
              statistic={{
                title: <h3>客户端总数</h3>,
                value: metrics.client_all,
                icon: <DatabaseOutlined style={{ fontSize: 40, color: '#66AFF4' }} />,
              }}
            />
          </Col>
          <Col {...topColResponsiveProps}>
            <StatisticCard
              statistic={{
                title: <h3>在线数量</h3>,
                value: metrics.client_online,
                icon: <CloudServerOutlined style={{ fontSize: 40, color: '#C7A9F0' }} />,
              }}
            />
          </Col>
          <Col {...topColResponsiveProps}>
            <StatisticCard
              statistic={{
                title: <h3>流入流量</h3>,
                value: formatBytes(metrics.traffic_in_total),
              }}
              chart={<Tiny.Ring {...getConfig(totalIn, '#66AFF4')} />}
              chartPlacement="left"
            />
          </Col>
          <Col {...topColResponsiveProps}>
            <StatisticCard
              statistic={{
                title: <h3>流出流量</h3>,
                value: formatBytes(metrics.traffic_out_total),
              }}
              chart={<Tiny.Ring {...getConfig(totalOut, '#62DAAA')} />}
              chartPlacement="left"
            />
          </Col>
        </Row>

        <Row gutter={24}>
          <Col xl={12} lg={24} md={24} sm={24} xs={24}>
            <Descriptions layout="horizontal" bordered>
              <Descriptions.Item label="Version" span={3}>
                {summary?.version}
              </Descriptions.Item>
              <Descriptions.Item label="BindPort" span={3}>
                {summary?.bindPort}
              </Descriptions.Item>
              <Descriptions.Item label="HttpPort" span={3}>
                {summary?.httpPort}
              </Descriptions.Item>
              <Descriptions.Item label="HttpsPort" span={3}>
                {summary?.httpsPort}
              </Descriptions.Item>
              <Descriptions.Item label="Proxies" span={3}>
                {summary?.proxies}
              </Descriptions.Item>
              <Descriptions.Item label="Certs" span={3}>
                {summary?.certs}
              </Descriptions.Item>
            </Descriptions>
          </Col>
          <Col xl={12} lg={24} md={24} sm={24} xs={24}>
            <Card bodyStyle={{ padding: 13 }}>
              <Descriptions layout="vertical">
                <Descriptions.Item label="cpu" span={3} style={{ paddingBottom: 0 }}>
                  <Progress
                    percent={~~metrics.system_cpu}
                    status="normal"
                    size="small"
                    strokeColor={metrics.system_cpu > 70 ? '#FF4D4F' : '#52C41A'}
                  />
                </Descriptions.Item>
                <Descriptions.Item label="memory" span={3} style={{ paddingBottom: 0 }}>
                  <Progress
                    percent={~~metrics.system_memory}
                    status="normal"
                    size="small"
                    strokeColor={metrics.system_memory > 70 ? '#FF4D4F' : '#52C41A'}
                  />
                </Descriptions.Item>
              </Descriptions>
              <Descriptions layout="horizontal">
                <Descriptions.Item label="tcp" span={3}>
                  {metrics.connection_tcp}
                </Descriptions.Item>
                <Descriptions.Item label="udp" span={3}>
                  {metrics.connection_udp}
                </Descriptions.Item>
                <Descriptions.Item label="in" span={3}>
                  {formatBytes(metrics.traffic_in_sec)}/s
                </Descriptions.Item>
                <Descriptions.Item label="out" span={3}>
                  {formatBytes(metrics.traffic_out_sec)}/s
                </Descriptions.Item>
                <Descriptions.Item label="total" span={3}>
                  {formatBytes(metrics.traffic_total_sec)}/s
                </Descriptions.Item>
              </Descriptions>
            </Card>
          </Col>
        </Row>
      </PageContainer>
    </SignalRContext.Provider>
  );
};

export default Overview;
