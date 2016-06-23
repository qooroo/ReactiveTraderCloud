import React from 'react';
import ReactDOM from 'react-dom';
import classnames from 'classnames';
import { router, logger } from '../../../system';
import { ViewBase } from '../../common';
import { OrdersModel } from '../model';
import numeral from 'numeral';
import Dimensions from 'react-dimensions';
import './orders.scss';
import {
  Direction,
  ExecuteTradeRequest,
  CurrencyPair,
} from '../../../services/model';

var _log: logger.Logger = logger.create('OrdersView');

@Dimensions()
export default class OrdersView extends ViewBase {
  constructor() {
    super();
    this.state = {
      model: null
    };
  }

  placeOrder(evt) {
    const direction = this.state.model.isSell ? Direction.Sell : Direction.Buy;
    const base = this.state.model.ccyPair.slice(0,3);
    console.log('placing order: ' + direction + ' ' + this.state.model.ccyPair + ' (base ' + base + ') ' + this.state.model.notional + ' @ ' + this.state.model.rate);
    let trade = new ExecuteTradeRequest(this.state.model.ccyPair, this.state.model.rate, direction.name, this.state.model.notional, base);
    router.publishEvent(this.props.modelId, 'placeOrder', { trade });
  }

  render() {

    let model = this.state.model;
    let isSell = model.isSell;
    let baseClassName = 'btn orders__buttons-tab-btn ';
    let selectedClassName = `${baseClassName} orders__buttons-tab-btn--selected`;
    let sellButtonClassName = isSell ? selectedClassName : baseClassName;
    let buyButtonClassName = isSell ? baseClassName : selectedClassName;

    return (
      <div className='orders__container animated fadeIn'>
        <div className='orders__header orders__header--bold orders__header-block'>
          ORDERS
        </div>
        <div className='orders__title-container'>
          <div className='orders__buttons-bar'>
            <button
              className={buyButtonClassName}
              onClick={() => router.publishEvent(this.props.modelId, 'toggleBuySellMode', {})}>BUY
            </button>
            <button
              className={sellButtonClassName}
              onClick={() => router.publishEvent(this.props.modelId, 'toggleBuySellMode', {})}>SELL
            </button>
          </div>
        </div>
        <div className='orders__form-element-container orders__form-spacer'>
          <div className='orders__form-element-label'>Currency Pair</div>
          <input
            className='orders__form-element-content orders__input'
            type='text'
            onChange={e => this.state.model.ccyPair = e.target.value} />
        </div>
        <div className='orders__form-element-container orders__form-spacer'>
          <div className='orders__form-element-label'>Rate</div>
          <input
            className='orders__form-element-content orders__input'
            type='text'
            onChange={e => this.state.model.rate = e.target.value}/>
        </div>
        <div className='orders__form-element-container orders__form-spacer'>
          <div className='orders__form-element-label'>Notional</div>
          <input
            className='orders__form-element-content orders__input'
            type='text'
            onChange={e => this.state.model.notional = e.target.value}/>
        </div>
        <div className='orders__title-container'>
          <button
            className='btn orders__form-btn'
            onClick={() => this.placeOrder()}>
            Place Order
          </button>
        </div>
      </div>);
  }
}
