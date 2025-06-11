import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$: Observable<boolean> = this.loadingSubject.asObservable();

  private activeRequests = 0;

  /**
   * Increments the count of active HTTP requests.
   * Sets loading state to true.
   */
  incrementRequest() {
    this.activeRequests++;
    if (this.activeRequests === 1) {
      this.loadingSubject.next(true);
      this.loadingSubject.subscribe(val => console.log('Loading state:', val));
    }
  }

  decrementRequest() {
    if (this.activeRequests > 0) {
      this.activeRequests--;
    }
    if (this.activeRequests === 0) {
      this.loadingSubject.next(false);
    }
  }

  /**
   * Internal method to update the BehaviorSubject.
   * @param isLoading True to show loading, false to hide.
   */
  setLoading(isLoading: boolean) {
    this.loadingSubject.next(isLoading);
  }
}
