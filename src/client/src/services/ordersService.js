import Rx from 'rx';
import { Connection, ServiceBase } from '../system/service';
import { Guard, logger, SchedulerService, RetryPolicy } from '../system';
import { ReferenceDataService } from './';

var _log:logger.Logger = logger.create('OrdersService');

export default class OrdersService extends ServiceBase {

  constructor(serviceType:string, connection:Connection, schedulerService:SchedulerService, referenceDataService:ReferenceDataService) {
    super(serviceType, connection, schedulerService);
  }
}
