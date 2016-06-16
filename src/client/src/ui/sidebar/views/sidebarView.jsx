import React from 'react';
import { ViewBase } from '../../common';
import { SidebarModel } from '../model';
import classnames from 'classnames';
import './sidebar.scss';

export default class SidebarView extends ViewBase {
  constructor() {
    super();
    this.state = {
      model: null
    };
  }

  render() {
    let model:SidebarModel = this.state.model;
    if (!model || !model.showSidebar) {
      return null;
    }

    let analyticsButtonClassNames = classnames (
      'sidebar__element-button  glyphicon glyphicon-stats',
      {
        'sidebar__element--active': model.showAnalytics,
        'sidebar__element--inactive' :  !model.showAnalytics
      }
    );

    let ordersButtonClassNames = classnames (
      'sidebar__element-button  glyphicon glyphicon-list',
      {
        'sidebar__element--active': model.showOrders,
        'sidebar__element--inactive' :  !model.showOrders
      }
    );

    return (
      <div className='sidebar__container'>
        <div className={analyticsButtonClassNames} onClick={() => model.toggleAnalyticsPanel()}/>
        <div className={ordersButtonClassNames} onClick={() => model.toggleOrdersPanel()}/>
        <div className='sidebar__element'></div>
      </div>
    );
  }
}
