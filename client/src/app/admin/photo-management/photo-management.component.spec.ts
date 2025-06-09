import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PhotoManagementComponent } from './photo-management.component';
import { AdminService } from '../../_services/admin.service';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

class MockAdminService {
  addTag = jasmine.createSpy().and.returnValue(of({}));
  getTags = jasmine.createSpy().and.returnValue(of([{ id: 1, name: 'tag1' }]));
  removeTagByName = jasmine.createSpy().and.returnValue(of({}));
  getPhotosForApproval = jasmine.createSpy().and.returnValue(of([]));
  approvePhoto = jasmine.createSpy().and.returnValue(of({}));
  rejectPhoto = jasmine.createSpy().and.returnValue(of({}));
}

class MockToastrService {
  success = jasmine.createSpy();
  error = jasmine.createSpy();
}

describe('PhotoManagementComponent', () => {
  let component: PhotoManagementComponent;
  let fixture: ComponentFixture<PhotoManagementComponent>;
  let adminService: MockAdminService;
  let toastr: MockToastrService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoManagementComponent, FormsModule],
      providers: [
        { provide: AdminService, useClass: MockAdminService },
        { provide: ToastrService, useClass: MockToastrService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PhotoManagementComponent);
    component = fixture.componentInstance;
    adminService = TestBed.inject(AdminService) as any;
    toastr = TestBed.inject(ToastrService) as any;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call addTag and show success on createTag', () => {
    const form = { invalid: false, resetForm: jasmine.createSpy() } as any;
    component.newTag = { name: 'tag1' } as any;
    component.createTag(form);
    expect(adminService.addTag).toHaveBeenCalledWith(component.newTag);
    expect(toastr.success).toHaveBeenCalledWith('Tag successfully created');
    expect(form.resetForm).toHaveBeenCalled();
  });

  it('should show error if addTag returns 400', () => {
    adminService.addTag.and.returnValue(throwError({ status: 400 }));
    const form = { invalid: false, resetForm: jasmine.createSpy() } as any;
    component.newTag = { name: 'tag1' } as any;
    component.createTag(form);
    expect(toastr.error).toHaveBeenCalledWith("You can't create duplicates!");
  });

  it('should call getTags and set tags', () => {
    component.getTags();
    expect(adminService.getTags).toHaveBeenCalled();
    expect(component.tags).toEqual([{ id: 1, name: 'tag1' }]);
  });

  it('should call removeTag and show success', () => {
    component.removeTag('tag1');
    expect(adminService.removeTagByName).toHaveBeenCalledWith('tag1');
    expect(toastr.success).toHaveBeenCalledWith('Tag successfully removed');
  });

  it('should show error if removeTag returns 400', () => {
    adminService.removeTagByName.and.returnValue(throwError({ status: 400 }));
    component.removeTag('tag1');
    expect(toastr.error).toHaveBeenCalledWith(
      "You can't remove a tag that is in use!"
    );
  });
});
