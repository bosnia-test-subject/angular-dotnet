import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { MembersService } from './members.service';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { ToastrService } from 'ngx-toastr';

class MockToastrService {
  success() {}
  error() {}
  info() {}
  warning() {}
}

describe('MembersService', () => {
  let service: MembersService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        MembersService,
        { provide: ToastrService, useClass: MockToastrService },
      ],
    });
    service = TestBed.inject(MembersService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch a member by username', () => {
    const mockMember: Member = { userName: 'user1' } as Member;

    service.getMember('user1').subscribe(member => {
      expect(member).toEqual(mockMember);
    });

    const req = httpMock.expectOne(baseUrl + 'users/user1');
    expect(req.request.method).toBe('GET');
    req.flush(mockMember);
  });

  it('should update a member', () => {
    const updateData: Member = {
      id: 1,
      userName: 'user1',
      age: 25,
      photoUrl: '',
      knownAs: '',
      created: new Date(),
      lastActive: new Date(),
      gender: '',
      introduction: 'Hello',
      lookingFor: '',
      interests: '',
      city: '',
      country: '',
      photos: [],
    };

    service.updateMember(updateData).subscribe(response => {
      expect(response).toEqual({});
    });

    const req = httpMock.expectOne(baseUrl + 'users');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateData);
    req.flush({});
  });
});
