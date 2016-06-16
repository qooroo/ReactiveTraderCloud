import React from 'react';
import ReactDOM from 'react-dom';
import classnames from 'classnames';
import { router, logger } from '../../../system';
import { ViewBase } from '../../common';
import { OrdersModel } from '../model';
import numeral from 'numeral';
import Dimensions from 'react-dimensions';
import './orders.scss';

var _log:logger.Logger = logger.create('OrdersView');

@Dimensions()
export default class OrdersView extends ViewBase {
  constructor() {
    super();
    this.state = {
      model: null
    };
  }

  componentDidMount(){
    
  }

  componentDidUpdate() {
    
  }

  render() {

    return (
        <div className='orders__container'>
          <div ref='ordersInnerContainer'>ORDERS</div>
        </div>);

    let model:OrdersModel = this.state.model;
    if (!model) {
      return null;
    }
    if (!model.isOrdersServiceConnected)
      return (
        <div className='orders__container'>
          <div ref='ordersInnerContainer'></div>
        </div>);

    let newWindowBtnClassName = classnames(
      'glyphicon glyphicon-new-window',
      {
        'orders__icon--tearoff' : !model.canPopout,
        'orders__icon--tearoff--hidden' : model.canPopout
      }
    );

    return (
      <div className='orders orders__container animated fadeIn'>
        <div className='orderes__controls popout__controls'>
          <i className={newWindowBtnClassName}
             onClick={() => router.publishEvent(this.props.modelId, 'popOutOrders', {})}/>
        </div>
        // content
      </div>);
  }
}
