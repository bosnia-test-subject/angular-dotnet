import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { MessageService } from './message.service';
import { environment } from '../../environments/environment';

describe('MessageService', () => {
  let service: MessageService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MessageService],
    });
    service = TestBed.inject(MessageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should delete a message', () => {
    const messageId = 1;
    service.deleteMessage(messageId).subscribe(response => {
      expect(response).toEqual({});
    });

    const req = httpMock.expectOne(baseUrl + `messages/${messageId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
