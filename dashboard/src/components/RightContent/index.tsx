import { QuestionCircleOutlined } from '@ant-design/icons';
import { SelectLang as UmiSelectLang } from '@umijs/max';
import React from 'react';

export type SiderTheme = 'light' | 'dark';

export const SelectLang = () => {
  return (
    <UmiSelectLang
      style={{
        padding: 4,
        color: '#FFFFFF',
      }}
    />
  );
};

export const Question = () => {
  return (
    <div
      style={{
        display: 'flex',
        height: 26,
        color: '#FFFFFF',
      }}
      onClick={() => {
        window.open('https://github.com/chaldea/rho-aias');
      }}
    >
      <QuestionCircleOutlined />
    </div>
  );
};
