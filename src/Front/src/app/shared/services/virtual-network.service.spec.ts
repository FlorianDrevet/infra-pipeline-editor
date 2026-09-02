import { TestBed } from '@angular/core/testing';

import { MethodEnum } from '../enums/method.enum';
import {
  CreateVirtualNetworkRequest,
  UpdateVirtualNetworkRequest,
  VirtualNetworkResponse,
} from '../interfaces/virtual-network.interface';
import { AxiosService } from './axios.service';
import { VirtualNetworkService } from './virtual-network.service';

const VIRTUAL_NETWORK_ID = 'virtual-network-1';
const VIRTUAL_NETWORK_ROUTE = '/virtual-network';
const VIRTUAL_NETWORK_BY_ID_ROUTE = `${VIRTUAL_NETWORK_ROUTE}/${VIRTUAL_NETWORK_ID}`;
const TEST_ADDRESS_SPACE = `${buildIpv4(10, 0, 0, 0)}/16`;
const TEST_DNS_SERVER = buildIpv4(10, 0, 0, 4);

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

  it('updates a virtual network through the singular backend member route', async () => {
    const request = createUpdateVirtualNetworkRequest();
    const response = createVirtualNetworkResponse();
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.update(VIRTUAL_NETWORK_ID, request);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.PUT,
      VIRTUAL_NETWORK_BY_ID_ROUTE,
      request,
    );
  });
});

function createCreateVirtualNetworkRequest(): CreateVirtualNetworkRequest {
  return {
    resourceGroupId: 'resource-group-1',
    name: 'demo-vnet',
    location: 'westeurope',
    isExisting: true,
  };
}

function createUpdateVirtualNetworkRequest(): UpdateVirtualNetworkRequest {
  return {
    name: 'demo-vnet',
    location: 'westeurope',
    environmentSettings: [
      {
        environmentName: 'Development',
        addressSpaces: [TEST_ADDRESS_SPACE],
        dnsServers: [TEST_DNS_SERVER],
        enableDdosProtection: true,
      },
    ],
  };
}

function createVirtualNetworkResponse(): VirtualNetworkResponse {
  return {
    id: VIRTUAL_NETWORK_ID,
    resourceGroupId: 'resource-group-1',
    name: 'demo-vnet',
    location: 'westeurope',
    environmentSettings: [],
    subnets: [],
    isExisting: true,
  };
}

function buildIpv4(octet1: number, octet2: number, octet3: number, octet4: number): string {
  return [octet1, octet2, octet3, octet4].join('.');
}