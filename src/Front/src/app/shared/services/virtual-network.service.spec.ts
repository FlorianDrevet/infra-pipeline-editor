import { TestBed } from '@angular/core/testing';

import { MethodEnum } from '../enums/method.enum';
import {
  CreateVirtualNetworkRequest,
  VirtualNetworkResponse,
} from '../interfaces/virtual-network.interface';
import { AxiosService } from './axios.service';
import { VirtualNetworkService } from './virtual-network.service';

const VIRTUAL_NETWORK_ID = 'virtual-network-1';
const VIRTUAL_NETWORK_ROUTE = '/virtual-network';
const VIRTUAL_NETWORK_BY_ID_ROUTE = `${VIRTUAL_NETWORK_ROUTE}/${VIRTUAL_NETWORK_ID}`;

describe('VirtualNetworkService', () => {
  let service: VirtualNetworkService;
  let axiosServiceSpy: jasmine.SpyObj<AxiosService>;

  beforeEach(() => {
    axiosServiceSpy = jasmine.createSpyObj<AxiosService>('AxiosService', ['request$']);

    TestBed.configureTestingModule({
      providers: [
        VirtualNetworkService,
        { provide: AxiosService, useValue: axiosServiceSpy },
      ],
    });

    service = TestBed.inject(VirtualNetworkService);
  });

  it('gets a virtual network through the singular backend route', async () => {
    const response = createVirtualNetworkResponse();
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.getById(VIRTUAL_NETWORK_ID);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.GET,
      VIRTUAL_NETWORK_BY_ID_ROUTE,
    );
  });

  it('creates a virtual network through the singular backend collection route', async () => {
    const request = createCreateVirtualNetworkRequest();
    const response = createVirtualNetworkResponse();
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.create(request);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.POST,
      VIRTUAL_NETWORK_ROUTE,
      request,
    );
  });
});

function createCreateVirtualNetworkRequest(): CreateVirtualNetworkRequest {
  return {
    resourceGroupId: 'resource-group-1',
    name: 'demo-vnet',
    location: 'westeurope',
    enableDdosProtection: false,
    isExisting: true,
  };
}

function createVirtualNetworkResponse(): VirtualNetworkResponse {
  return {
    id: VIRTUAL_NETWORK_ID,
    resourceGroupId: 'resource-group-1',
    name: 'demo-vnet',
    location: 'westeurope',
    enableDdosProtection: false,
    environmentSettings: [],
    subnets: [],
    isExisting: true,
  };
}