import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { AdminService } from './admin.service';
import { environment } from '../../environments/environment';
import { Tag } from '../_models/tag';

describe('AdminService', () => {
  let service: AdminService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AdminService],
    });
    service = TestBed.inject(AdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch tags', () => {
    const mockTags: Tag[] = [
      { id: 1, name: 'tag1' },
      { id: 2, name: 'tag2' },
    ];
    service.getTags().subscribe(tags => {
      expect(tags).toEqual(mockTags);
    });
    const req = httpMock.expectOne(baseUrl + 'admin/get-tags');
    expect(req.request.method).toBe('GET');
    req.flush(mockTags);
  });

  it('should add a tag', () => {
    const newTag: Tag = { id: 1, name: 'newTag' };
    service.addTag(newTag).subscribe();
    const req = httpMock.expectOne(baseUrl + 'admin/create-tag');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newTag);
    req.flush({});
  });

  it('should remove a tag by name', () => {
    const tagName = 'tagToRemove';
    service.removeTagByName(tagName).subscribe();
    const req = httpMock.expectOne(baseUrl + `admin/remove-tag/${tagName}`);
    expect(req.request.method).toBe('DELETE');
    req.flush('Tag removed');
  });

  it('should approve a photo', () => {
    const photoId = 123;
    service.approvePhoto(photoId).subscribe();
    const req = httpMock.expectOne(baseUrl + `admin/approve-photo/${photoId}`);
    expect(req.request.method).toBe('POST');
    req.flush('Photo approved');
  });
});
