import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { AccountService } from './account.service';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';

class MockToastrService {
  success() {}
  error() {}
  info() {}
  warning() {}
}

describe('AccountService', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AccountService,
        { provide: ToastrService, useClass: MockToastrService },
      ],
    });
    service = TestBed.inject(AccountService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.match(() => true).forEach(req => req.flush({}));
    httpMock.verify();
  });

  it('should register a user', () => {
    const mockUser: User = { username: 'newuser', token: 'xyz789' } as User;
    const registrationData = { username: 'newuser', password: 'pass' };

    service.register(registrationData).subscribe(user => {
      expect(user).toEqual(mockUser);
    });

    const req = httpMock.expectOne(baseUrl + 'account/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(registrationData);
    req.flush(mockUser);
  });
});
