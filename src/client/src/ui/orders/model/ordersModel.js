import { Router,  observeEvent } from 'esp-js/src';
import { OrdersService } from '../../../services';
import { ServiceStatus } from '../../../system/service';
import { logger, Environment } from '../../../system';
import { ModelBase, RegionManagerHelper } from '../../common';
import { RegionManager, RegionNames, view  } from '../../regions';
import { RegionSettings } from '../../../services/model';
import { OrdersView } from '../views';
import { OpenFin } from '../../../system/openFin';
import { WellKnownModelIds } from '../../../';

var _log:logger.Logger = logger.create('OrdersModel');

@view(OrdersView)
export default class OrdersModel extends ModelBase {
  _ordersService:OrdersService;
  _regionManagerHelper:RegionManagerHelper;
  _regionManager:RegionManager;
  _regionSettings:RegionSettings;
  _regionName:string;
  _openFin:OpenFin;

  isOrdersServiceConnected: Boolean;

  constructor(
    modelId:string,
    router:Router,
    ordersService:OrdersService,
    regionManager:RegionManager,
    openFin:OpenFin
  ) {
    super(modelId, router);
    this._ordersService = ordersService;
    this._regionName = RegionNames.orders;
    this.isOrdersServiceConnected = false;
    this._regionSettings = new RegionSettings('Orders', 400, 800, false);
    this._regionManager = regionManager;
    this._regionManagerHelper = new RegionManagerHelper(this._regionName, regionManager, this, this._regionSettings);
    this._openFin = openFin;
  }

  get canPopout() {
    return Environment.isRunningInIE;
  }

//   observeEvents() {
//     super.observeEvents();
//     this.addDisposable(this.router.observeEventsOn(this._modelId));
//   }

  @observeEvent('init')
  _onInit() {
    _log.info(`Orders model starting`);
    this._subscribeToConnectionStatus();
    this._regionManagerHelper.init();
    this._observeSidebarEvents();
  }

//   @observeEvent('referenceDataLoaded')
//   _onReferenceDataLoaded() {
//     _log.info(`Ref data loaded, subscribing to orders stream`);
//     // todo
//   }

//   @observeEvent('popOutOrders')
//   _onPopOutOrders() {
//     _log.info(`Popping out orders`);
//     this._regionManagerHelper.popout();
//   }

  _subscribeToConnectionStatus() {
    this.addDisposable(
      this._ordersService.serviceStatusStream.subscribeWithRouter(
        this.router,
        this.modelId,
        (status:ServiceStatus) => {
          this.isOrdersServiceConnected = status.isConnected;
        })
    );
  }

  _observeSidebarEvents(){
    this.addDisposable(
      this.router
        .getEventObservable(WellKnownModelIds.sidebarModelId, 'hideOrders')
        .observe(() => this.router.runAction(this.modelId, ()=> {
          this._regionManagerHelper.removeFromRegion();
        }))
    );
    this.addDisposable(
      this.router
        .getEventObservable(WellKnownModelIds.sidebarModelId, 'showOrders')
        .observe(() => this.router.runAction(this.modelId, () => {
          this._regionManagerHelper.addToRegion();
        }))
    );
  }
}
